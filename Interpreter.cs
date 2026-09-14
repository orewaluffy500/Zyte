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
            Visit(node, context);
        }
    }

    public ZValue Visit(ASTNode node, InterpretationContext context)
    {
        // Console.WriteLine(node);

        // LITERALS & VALUES
        if (node is IntegerNode intNode)                                    return VisitInteger(intNode, context);
        else if (node is RegisterNode registerNode)                         return VisitRegister(registerNode, context);
        else if (node is RegisterAccessNode registerAccessNode)             return VisitRegisterAccess(registerAccessNode, context);
        else if (node is UnaryOperNode unaryOperNode)                       return VisitUnaryOper(unaryOperNode, context);
        else if (node is BinaryOperNode binaryOperNode)                     return VisitBinaryOper(binaryOperNode, context);
        else if (node is ChangeValueNode changeValueNode)                   return VisitChangeValue(changeValueNode, context);
        else if (node is StringNode stringNode)                             return VisitString(stringNode, context);
        
        // FLOW-RELATED
        else if (node is BodyNode bodyNode)                                 return VisitBody(bodyNode, context);
        else if (node is IfNode ifNode)                                     return VisitIfStatement(ifNode, context);
        else if (node is TernaryIfNode ternaryIfNode)                       return VisitTernaryIf(ternaryIfNode, context);

        // LOOP-RELATED
        else if (node is WhileNode whileNode)                               return VisitWhile(whileNode, context);
        else if (node is BreakNode breakNode)                               return VisitBreak(breakNode, context);
        else if (node is NextIterationNode nextIterationNode)               return VisitNextIteration(nextIterationNode, context);

        // INPUT / OUTPUT
        else if (node is PrintNode printNode)                               return VisitPrintNode(printNode, context);
        else if (node is ReadIntegerNode readIntegerNode)                   return VisitReadInteger(readIntegerNode, context);
        else if (node is ReadCharNode readCharNode)                         return VisitReadChar(readCharNode, context);
        else if (node is ReadKeyNode readKeyNode)                           return VisitReadKey(readKeyNode, context);

        // MEMORY-RELATED (REGISTERS)
        else if (node is CopyValueNode copyValueNode)                       return VisitCopyNode(copyValueNode, context);

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

    public ZValue VisitBody(BodyNode node, InterpretationContext context)
    {
        ZValue value = new ZNull(node.Pos.Copy());
        ASTNode[] body = node.Tree;

        foreach (ASTNode subNode in body)
        {
            ZValue output = Visit(subNode, context);
            if (!context.ShouldContinue){
                return value;
            }
            
            value = output;
        }

        return value;
    }

    // LITERALS & VALUES

    public ZValue VisitRegister(RegisterNode node, InterpretationContext context)
    {
        ZInt index = Expect<ZInt>(Visit(node.Index, context));

        return new ZRegister(index.Value)
        {
            Pos = node.Pos
        };
    }

    public ZValue VisitRegisterAccess(RegisterAccessNode node, InterpretationContext context)
    {
        ZInt index = Expect<ZInt>(Visit(node.Index, context));
        ZInt value = Memory.GetRegister(index.Value);

        return value;
    }

    public ZValue VisitInteger(IntegerNode node, InterpretationContext context)
    {
        return new ZInt((int)node.Value.Value!)
        {
            Pos = node.Pos.Copy()
        };
    }

    public ZValue VisitString(StringNode node, InterpretationContext context)
    {
        return new ZString((string)node.Value.Value!)
        {
            Pos = node.Pos.Copy()
        };
    }

    public ZValue VisitChangeValue(ChangeValueNode node, InterpretationContext context)
    {
        ZValue value = Visit(node.Value, context);

        if (node.OperToken.Type == TokenType.Increment) return value.Increment().Repos(node.Pos);
        else if (node.OperToken.Type == TokenType.Decrement) return value.Decrement().Repos(node.Pos);

        return value.Repos(node.Pos);
    }

    public ZValue VisitUnaryOper(UnaryOperNode node, InterpretationContext context)
    {
        ZValue value = Visit(node.Value, context);

        if (node.OperToken.Type == TokenType.Minus) return value.Negate().Repos(node.Pos);
        else if (node.OperToken.Type == TokenType.Plus) return value.Positate().Repos(node.Pos);

        return value.Repos(node.Pos);
    }

    public ZValue VisitBinaryOper(BinaryOperNode node, InterpretationContext context)
    {
        Token oper = node.OperToken;
        ZValue left = Visit(node.Left, context);
        ZValue right = Visit(node.Right, context);

        Position pos = node.Pos.Copy();

        if (oper.IsKeyword("lt")) return left.IsLessThan(right).Repos(pos);
        else if (oper.IsKeyword("gt")) return left.IsGreaterThan(right).Repos(pos);
        else if (oper.IsKeyword("eq")) return left.IsEqualTo(right).Repos(pos);

        return new ZNull(pos);
    }


    // MEMORY-RELATED (REGISTERS)

    public ZValue VisitCopyNode(CopyValueNode node, InterpretationContext context)
    {
        ZRegister register = Expect<ZRegister>(Visit(node.Register, context));

        ZInt value = Expect<ZInt>(Visit(node.Value, context));

        Memory.SetRegister(register.Index, value);

        return new ZNull(node.Pos.Copy());
    }



    // INPUT / OUTPUT

    public ZValue VisitPrintNode(PrintNode node, InterpretationContext context)
    {
        ZValue message = Visit(node.Value, context);

        Console.Write(message);
        if (node.Newline) Console.WriteLine();

        return new ZNull(node.Pos.Copy());
    }

    public ZValue VisitReadInteger(ReadIntegerNode node, InterpretationContext context)
    {
        ZRegister output = Expect<ZRegister>(Visit(node.Output, context));

        string input = Console.ReadLine() ?? "0";
        int result = int.TryParse(input, out int i) ? i : 0;

        Memory.SetRegister(output.Index, result);

        return new ZNull(node.Pos.Copy());
    }

    public ZValue VisitReadChar(ReadCharNode node, InterpretationContext context)
    {
        ZRegister output = Expect<ZRegister>(Visit(node.Output, context));

        int result = Console.Read();

        Memory.SetRegister(output.Index, result);
        return new ZNull(node.Pos.Copy());
    }

    public ZValue VisitReadKey(ReadKeyNode node, InterpretationContext context)
    {
        ZRegister output = Expect<ZRegister>(Visit(node.Output, context));

        var key = Console.ReadKey();
        Console.WriteLine();

        Memory.SetRegister(output.Index, key.KeyChar);

        // Return 1 if the pressed key was a modifier, 0 if not.
        return new ZInt(char.IsControl(key.KeyChar) ? 1 : 0)
        {
            Pos = node.Pos.Copy()
        };
    }

    // LOOP-RELATED


    public ZValue VisitBreak(BreakNode node, InterpretationContext context)
    {
        if (context is not LoopContext lc)
        {
            ErrorHandler.RTError("unexpected 'break'", "unexpected 'break' statement outside of loops", node.Pos);
        } else {
            lc.Running = false;
        }

        return new ZNull(node.Pos.Copy());
    }

    public ZValue VisitNextIteration(NextIterationNode node, InterpretationContext context)
    {
        if (context is not LoopContext lc)
        {
            ErrorHandler.RTError("unexpected 'next'", "unexpected 'next' statement outside of loops", node.Pos);
        } else {
            lc.Continue = true;
        }

        return new ZNull(node.Pos.Copy());
    }




    // FLOW-RELATED

    public ZValue VisitIfStatement(IfNode node, InterpretationContext context)
    {
        int index = 0;
        ZValue result = new ZNull(node.Pos.Copy());

        while (index < node.Cases.Length)
        {
            IfCase ifCase = node.Cases[index];

            ZValue conditionOutput = Visit(ifCase.Condition, context);

            if (conditionOutput.IsTrue())
            {
                result = Visit(ifCase.Body, context);
                return result.Repos(node.Pos.Copy());
            }

            index++;
        }

        if (node.ElseCase is not null)
        {
            result = Visit(node.ElseCase.Body, context);
        }

        return result.Repos(node.Pos.Copy());
    }

    public ZValue VisitWhile(WhileNode node, InterpretationContext context)
    {
        ZValue conditionOutput = Visit(node.Condition, context);
        ZValue result = new ZNull(node.Pos.Copy());

        WhileLoopContext loopContext = new((BodyNode) node.Body, conditionOutput.IsTrue());

        while (loopContext.Running)
        {
            loopContext.Running = Visit(node.Condition, loopContext).IsTrue();
            
            foreach (ASTNode subNode in loopContext.Body.Tree)
            {
                ZValue output = Visit(subNode, loopContext);
                
                if (!loopContext.ShouldContinue)
                {
                    // if do continue then set it false otherwise keep it false
                    loopContext.Continue = loopContext.Continue && false;
                    break;
                }

                result = output;
            }
        }

        return result.Repos(node.Pos.Copy());
    }



    public ZValue VisitTernaryIf(TernaryIfNode node, InterpretationContext context)
    {
        ZValue conditionOutput = Visit(node.Condition, context);

        if (conditionOutput.IsTrue())
        {
            return Visit(node.HappyCase, context);
        }

        return Visit(node.BadCase, context);
    }
}