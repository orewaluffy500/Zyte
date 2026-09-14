namespace zyte;

class Interpreter(BodyNode body)
{
    public ExecutionFrame ExecutionFrame = new(body);
    public ASTNode Current { get => ExecutionFrame.Current; }
    public Memory Memory = new(80);
    public bool IsEnd { get => Current is EndNode; }

    public void Next()
    {
        ExecutionFrame.Index++;
    }

    public void Interpret()
    {
        while (true)
        {
            if (IsEnd)
            {
                if (ExecutionFrame.IsTop) break;
                ExecutionFrame = ExecutionFrame.Back!;
                Next();
            }

            Visit(Current);
            Next();
        }
    }

    public ZValue Visit(ASTNode node)
    {
        // Console.WriteLine(node);

        // LITERALS & VALUES
        if (node is IntegerNode intNode) return VisitInteger(intNode);
        else if (node is RegisterNode registerNode) return VisitRegister(registerNode);
        else if (node is RegisterAccessNode registerAccessNode) return VisitRegisterAccess(registerAccessNode);
        else if (node is UnaryOperNode unaryOperNode) return VisitUnaryOper(unaryOperNode);
        else if (node is StringNode stringNode) return VisitString(stringNode);
        
        // FLOW-RELATED
        else if (node is BodyNode bodyNode) return VisitBody(bodyNode);
        else if (node is IfNode ifNode) return VisitIfStatement(ifNode);
        else if (node is TernaryIfNode ternaryIfNode) return VisitTernaryIf(ternaryIfNode);

        // INPUT / OUTPUT
        else if (node is PrintNode printNode) return VisitPrintNode(printNode);
        else if (node is ReadIntegerNode readIntegerNode) return VisitReadInteger(readIntegerNode);
        else if (node is ReadCharNode readCharNode) return VisitReadChar(readCharNode);
        else if (node is ReadKeyNode readKeyNode) return VisitReadKey(readKeyNode);

        // MEMORY-RELATED (REGISTERS)
        else if (node is CopyValueNode copyValueNode) return VisitCopyNode(copyValueNode);

        return new ZNull(node.Pos.Copy());
    }

    /* ---- HELPERS  ----------------------------- */

    public T Expect<T>(ZValue value) where T : ZValue
    {
        if (value is T) return value.To<T>();

        ErrorHandler.RTError("type mismatch", $"expected value of type {ZValue.GetLabelOf(typeof(T))}, got {ZValue.GetLabelOf(value.GetType())}", value.Pos);

        return value.To<T>(); // impossible since error handler aborts.
    }

    /* ---- VISITORS ----------------------------- */

    public ZValue VisitBody(BodyNode node)
    {
        ExecutionFrame = new(node, ExecutionFrame)
        {
            Index = -1
        };

        return new ZNull(node.Pos.Copy());
    }

    // LITERALS & VALUES

    public ZValue VisitRegister(RegisterNode node)
    {
        ZInt index = Expect<ZInt>(Visit(node.Index));

        return new ZRegister(index.Value)
        {
            Pos = node.Pos
        };
    }

    public ZValue VisitRegisterAccess(RegisterAccessNode node)
    {
        ZInt index = Expect<ZInt>(Visit(node.Index));
        ZInt value = Memory.GetRegister(index.Value);

        return value;
    }

    public ZValue VisitInteger(IntegerNode node)
    {
        return new ZInt((int)node.Value.Value!)
        {
            Pos = node.Pos.Copy()
        };
    }

    public ZValue VisitString(StringNode node)
    {
        return new ZString((string)node.Value.Value!)
        {
            Pos = node.Pos.Copy()
        };
    }

    public ZValue VisitUnaryOper(UnaryOperNode node)
    {
        ZValue value = Visit(node.Value);

        if (node.OperToken.Type == TokenType.Minus) return value.Negate();
        else if (node.OperToken.Type == TokenType.Plus) return value.Positate();

        return value;
    }


    // MEMORY-RELATED (REGISTERS)

    public ZValue VisitCopyNode(CopyValueNode node)
    {
        ZRegister register = Expect<ZRegister>(Visit(node.Register));

        ZInt value = Expect<ZInt>(Visit(node.Value));

        Memory.SetRegister(register.Index, value);

        return new ZNull(node.Pos.Copy());
    }



    // INPUT / OUTPUT

    public ZValue VisitPrintNode(PrintNode node)
    {
        ZValue message = Visit(node.Value);

        Console.Write(message);
        if (node.Newline) Console.WriteLine();

        return new ZNull(node.Pos.Copy());
    }

    public ZValue VisitReadInteger(ReadIntegerNode node)
    {
        ZRegister output = Expect<ZRegister>(Visit(node.Output));

        string input = Console.ReadLine() ?? "0";
        int result = int.TryParse(input, out int i) ? i : 0;

        Memory.SetRegister(output.Index, result);

        return new ZNull(node.Pos.Copy());
    }

    public ZValue VisitReadChar(ReadCharNode node)
    {
        ZRegister output = Expect<ZRegister>(Visit(node.Output));

        int result = Console.Read();

        Memory.SetRegister(output.Index, result);
        return new ZNull(node.Pos.Copy());
    }

    public ZValue VisitReadKey(ReadKeyNode node)
    {
        ZRegister output = Expect<ZRegister>(Visit(node.Output));

        var key = Console.ReadKey();
        Console.WriteLine();

        Memory.SetRegister(output.Index, key.KeyChar);

        // Return 1 if the pressed key was a modifier, 0 if not.
        return new ZInt(char.IsControl(key.KeyChar) ? 1 : 0)
        {
            Pos = node.Pos.Copy()
        };
    }


    // FLOW-RELATED

    public ZValue VisitIfStatement(IfNode node)
    {
        int index = 0;

        while (index < node.Cases.Length)
        {
            IfCase ifCase = node.Cases[index];

            ZValue conditionOutput = Visit(ifCase.Condition);

            if (conditionOutput.IsTrue())
            {
                Visit(ifCase.Body);
                return new ZNull(node.Pos.Copy());
            }

            index++;
        }

        if (node.ElseCase is not null)
        {
            Visit(node.ElseCase.Body);
        }

        return new ZNull(node.Pos.Copy());
    }



    public ZValue VisitTernaryIf(TernaryIfNode node)
    {
        ZValue conditionOutput = Visit(node.Condition);

        if (conditionOutput.IsTrue())
        {
            return Visit(node.HappyCase);
        }

        return Visit(node.BadCase);
    }
}