namespace zyte;

class Interpreter(BodyNode body)
{
    public BodyNode Program = body;
    public Memory Memory = new(80);
    public void Interpret()
    {
        ASTNode[] programBody = Program.Tree;
        InterpretationContext context = new(Program);
        
        foreach (ASTNode node in programBody)
        {
            Visit(node);
        }
    }

    public VisitResult Visit(ASTNode node)
    {
        // Console.WriteLine(node);

        // LITERALS & VALUES
        if (node is IntegerNode intNode)                                    return VisitInteger(intNode);
        else if (node is RegisterNode registerNode)                         return VisitRegister(registerNode);
        else if (node is RegisterAccessNode registerAccessNode)             return VisitRegisterAccess(registerAccessNode);
        else if (node is UnaryOperNode unaryOperNode)                       return VisitUnaryOper(unaryOperNode);
        else if (node is BinaryOperNode binaryOperNode)                     return VisitBinaryOper(binaryOperNode);
        else if (node is NotNode notNode)                                   return VisitNot(notNode);
        else if (node is ChangeValueNode changeValueNode)                   return VisitChangeValue(changeValueNode);
        else if (node is StringNode stringNode)                             return VisitString(stringNode);
        
        // FLOW-RELATED
        else if (node is BodyNode bodyNode)                                 return VisitBody(bodyNode);
        else if (node is IfNode ifNode)                                     return VisitIfStatement(ifNode);
        else if (node is TernaryIfNode ternaryIfNode)                       return VisitTernaryIf(ternaryIfNode);

        // LOOP-RELATED
        else if (node is WhileNode whileNode)                               return VisitWhile(whileNode);
        else if (node is BreakNode breakNode)                               return VisitBreak(breakNode);
        else if (node is NextIterationNode nextIterationNode)               return VisitNextIteration(nextIterationNode);

        // INPUT / OUTPUT
        else if (node is PrintNode printNode)                               return VisitPrintNode(printNode);
        else if (node is ReadIntegerNode readIntegerNode)                   return VisitReadInteger(readIntegerNode);
        else if (node is ReadCharNode readCharNode)                         return VisitReadChar(readCharNode);
        else if (node is ReadKeyNode readKeyNode)                           return VisitReadKey(readKeyNode);

        // MEMORY-RELATED (REGISTERS)
        else if (node is CopyValueNode copyValueNode)                       return VisitCopyNode(copyValueNode);

        return new ZNull(node.Pos);
    }

    /* ---- HELPERS  ----------------------------- */

    public T Expect<T>(ZValue value) where T : ZValue
    {
        if (value is T) return value.To<T>();

        ErrorHandler.RTError("type mismatch", $"expected value of type {ZValue.GetLabelOf(typeof(T))}, got {ZValue.GetLabelOf(value.GetType())}", value.Pos);

        return value.To<T>(); // impossible since error handler aborts.
    }

    /* ---- VISITORS ----------------------------- */

    public VisitResult VisitBody(BodyNode node)
    {
        ZValue value = new ZNull(node.Pos);
        ASTNode[] body = node.Tree;

        foreach (ASTNode subNode in body)
        {
            VisitResult result = Visit(subNode);
            if (result.Flow != ExecutionFlow.Normal) return new(value, result.Flow);

            value = result.Value;
        }

        return value;
    }

    // LITERALS & VALUES

    public VisitResult VisitRegister(RegisterNode node)
    {
        ZInt index = Expect<ZInt>(Visit(node.Index));

        return new ZRegister(index.Value)
        {
            Pos = node.Pos
        };
    }

    public VisitResult VisitRegisterAccess(RegisterAccessNode node)
    {
        ZInt index = Expect<ZInt>(Visit(node.Index));
        ZInt value = Memory.GetRegister(index.Value);

        return value;
    }

    public VisitResult VisitInteger(IntegerNode node)
    {
        return new ZInt((int)node.Value.Value!)
        {
            Pos = node.Pos
        };
    }

    public VisitResult VisitString(StringNode node)
    {
        return new ZString((string)node.Value.Value!)
        {
            Pos = node.Pos
        };
    }

    public VisitResult VisitChangeValue(ChangeValueNode node)
    {
        ZValue value = Visit(node.Value);

        if (node.OperToken.Type == TokenType.Increment) return value.Increment().Repos(node.Pos);
        else if (node.OperToken.Type == TokenType.Decrement) return value.Decrement().Repos(node.Pos);

        return value.Repos(node.Pos);
    }

    public VisitResult VisitUnaryOper(UnaryOperNode node)
    {
        ZValue value = Visit(node.Value);

        if (node.OperToken.Type == TokenType.Minus) return value.Negate().Repos(node.Pos);
        else if (node.OperToken.Type == TokenType.Plus) return value.Positate().Repos(node.Pos);

        return value.Repos(node.Pos);
    }

    public VisitResult VisitBinaryOper(BinaryOperNode node)
    {
        Token oper = node.OperToken;
        ZValue left = Visit(node.Left);
        ZValue right = Visit(node.Right);

        Position pos = node.Pos;

        if (oper.IsKeyword("lt")) return left.IsLessThan(right).Repos(pos);
        else if (oper.IsKeyword("gt")) return left.IsGreaterThan(right).Repos(pos);
        else if (oper.IsKeyword("eq")) return left.IsEqualTo(right).Repos(pos);
        else if (oper.IsKeyword("and")) return left.Anded(right);
        else if (oper.IsKeyword("or")) return left.Ored(right);
        else if (oper.IsKeyword("xor")) return left.ExclOred(right);

        return new ZNull(pos);
    }

    public VisitResult VisitNot(NotNode node)
    {
        VisitResult value = Visit(node.Value);

        return value.Value.Notted();
    }


    // MEMORY-RELATED (REGISTERS)

    public VisitResult VisitCopyNode(CopyValueNode node)
    {
        ZRegister register = Expect<ZRegister>(Visit(node.Register));

        ZInt value = Expect<ZInt>(Visit(node.Value));

        Memory.SetRegister(register.Index, value);

        return new ZNull(node.Pos);
    }



    // INPUT / OUTPUT

    public VisitResult VisitPrintNode(PrintNode node)
    {
        ZValue message = Visit(node.Value);

        Console.Write(message);
        if (node.Newline) Console.WriteLine();

        return new ZNull(node.Pos);
    }

    public VisitResult VisitReadInteger(ReadIntegerNode node)
    {
        ZRegister output = Expect<ZRegister>(Visit(node.Output));

        string input = Console.ReadLine() ?? "0";
        int result = int.TryParse(input, out int i) ? i : 0;

        Memory.SetRegister(output.Index, result);

        return new ZNull(node.Pos);
    }

    public VisitResult VisitReadChar(ReadCharNode node)
    {
        ZRegister output = Expect<ZRegister>(Visit(node.Output));

        int result = Console.Read();

        Memory.SetRegister(output.Index, result);
        return new ZNull(node.Pos);
    }

    public VisitResult VisitReadKey(ReadKeyNode node)
    {
        ZRegister output = Expect<ZRegister>(Visit(node.Output));

        var key = Console.ReadKey();
        Console.WriteLine();

        Memory.SetRegister(output.Index, key.KeyChar);

        // Return 1 if the pressed key was a modifier, 0 if not.
        return new ZInt(char.IsControl(key.KeyChar) ? 1 : 0)
        {
            Pos = node.Pos
        };
    }

    // LOOP-RELATED


    public VisitResult VisitBreak(BreakNode node)
    {
        return new(new ZNull(node.Pos), ExecutionFlow.Break);
    }

    public VisitResult VisitNextIteration(NextIterationNode node)
    {
        return new(new ZNull(node.Pos), ExecutionFlow.Continue);
    }




    // FLOW-RELATED

    public VisitResult VisitIfStatement(IfNode node)
    {
        int index = 0;
        VisitResult result = new(new ZNull(node.Pos));

        while (index < node.Cases.Length)
        {
            IfCase ifCase = node.Cases[index];

            ZValue conditionOutput = Visit(ifCase.Condition);

            if (conditionOutput.IsTrue())
            {
                result = Visit(ifCase.Body);
                return result.Repos(ifCase.Body.Pos);
            }

            index++;
        }

        if (node.ElseCase is not null)
        {
            result = Visit(node.ElseCase.Body);
        }

        return result.Repos(node.Pos);
    }

    public VisitResult VisitWhile(WhileNode node)
    {
        ZValue conditionOutput = Visit(node.Condition);
        ZValue finalValue = new ZNull(node.Pos);

        while (conditionOutput.IsTrue())
        {
            VisitResult result = Visit(node.Body);
            if (result.Flow == ExecutionFlow.Break) break;

            finalValue = result.Value;
            conditionOutput = Visit(node.Condition);
        }

        return finalValue.Repos(node.Pos);
    }



    public VisitResult VisitTernaryIf(TernaryIfNode node)
    {
        ZValue conditionOutput = Visit(node.Condition);

        if (conditionOutput.IsTrue())
        {
            return Visit(node.HappyCase);
        }

        return Visit(node.BadCase);
    }
}