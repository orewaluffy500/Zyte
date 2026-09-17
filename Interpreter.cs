namespace zyte;

class Interpreter(BodyNode body)
{
    public BodyNode Program = body;
    public Memory Memory = new(80);
    public List<CallFrame> CallStack = [];
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
        else if (node is DereferenceNode dereferenceNode)                   return VisitDereference(dereferenceNode);
        else if (node is DereferenceExtNode dereferenceExtNode)             return VisitDereferenceExt(dereferenceExtNode);
        else if (node is UnaryOperNode unaryOperNode)                       return VisitUnaryOper(unaryOperNode);
        else if (node is BinaryOperNode binaryOperNode)                     return VisitBinaryOper(binaryOperNode);
        else if (node is NotNode notNode)                                   return VisitNot(notNode);
        else if (node is ChangeValueNode changeValueNode)                   return VisitChangeValue(changeValueNode);
        else if (node is StringNode stringNode)                             return VisitString(stringNode);
        else if (node is DiscardCaptureNode discardCaptureNode)             return VisitDiscardCapture(discardCaptureNode);
        
        // FLOW-RELATED
        else if (node is BodyNode bodyNode)                                 return VisitBody(bodyNode);
        else if (node is IfNode ifNode)                                     return VisitIfStatement(ifNode);
        else if (node is TernaryIfNode ternaryIfNode)                       return VisitTernaryIf(ternaryIfNode);

        // LOOP-RELATED
        else if (node is ForNode forNode)                                   return VisitFor(forNode);
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

        // FUNC-RELATED
        else if (node is SymbolAccessNode symbolAccessNode)                 return VisitSymbolAccess(symbolAccessNode);
        else if (node is ArgumentAccessNode argumentAccessNode)             return VisitArgumentAccess(argumentAccessNode);
        else if (node is FuncOutputAccessNode funcOutputAccessNode)         return VisitFuncOutputAccess(funcOutputAccessNode);
        else if (node is ReturnNode returnNode)                             return VisitReturn(returnNode);
        else if (node is CallNode callNode)                                 return VisitCall(callNode); 
        else if (node is FuncDefNode funcDefNode)                           return VisitFuncDef(funcDefNode);
        
        // ARRAY-RELATED
        else if (node is ReserveArrayNode reserveArrayNode)                 return VisitReserveArray(reserveArrayNode);
        else if (node is ArrayNode arrayNode)                               return VisitArray(arrayNode);

        return new ZNull(node.Pos);
    }

    /* ---- HELPERS  ----------------------------- */

    public T Expect<T>(ZValue value) where T : ZValue
    {
        if (value is T) return value.To<T>();

        ErrorHandler.RTError("type mismatch", $"expected {ZValue.GetLabelOf(typeof(T))}, got {ZValue.GetLabelOf(value.GetType())}", value.Pos);

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

        return new ZRegister(index.Value, this)
        {
            Pos = node.Pos
        };
    }

    public VisitResult VisitDiscardCapture(DiscardCaptureNode node)
    {
        return new ZDiscardCapture(this)
        {
            Pos = node.Pos
        };
    }

    public VisitResult VisitDereference(DereferenceNode node)
    {
        ZValue value = Visit(node.Index);

        if (value is ZRegister r) return Memory.GetRegister(r.Index);
        else if (value is ZInt i) return Memory.GetRegister(i.Value);

        return new ZNull(node.Pos);
    }

    public VisitResult VisitDereferenceExt(DereferenceExtNode node)
    {
        ZInt value = Expect<ZInt>(Visit(node.Address));

        return Memory.GetExternal(value.Value, node.Pos);
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
        ZCapture capture = Expect<ZCapture>(Visit(node.Register));

        ZValue value = Visit(node.Value);

        capture.Set(value);

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
        ZCapture output = Expect<ZCapture>(Visit(node.Output));

        string input = Console.ReadLine() ?? "0";
        int result = int.TryParse(input, out int i) ? i : 0;

        output.Set(new ZInt(result){ Pos = node.Pos });

        return new ZNull(node.Pos);
    }

    public VisitResult VisitReadChar(ReadCharNode node)
    {
        ZCapture output = Expect<ZCapture>(Visit(node.Output));

        int result = Console.Read();

        output.Set(new ZInt(result){ Pos = node.Pos });
        return new ZNull(node.Pos);
    }

    public VisitResult VisitReadKey(ReadKeyNode node)
    {
        ZCapture output = Expect<ZCapture>(Visit(node.Output));

        var key = Console.ReadKey();
        Console.WriteLine();

        output.Set(new ZInt(key.KeyChar){ Pos = node.Pos });

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
            else if (result.Flow != ExecutionFlow.Normal && result.Flow != ExecutionFlow.Continue) return result;

            finalValue = result.Value;
            conditionOutput = Visit(node.Condition);
        }

        return finalValue.Repos(node.Pos);
    }

    public VisitResult VisitFor(ForNode node)
    {
        ZCapture capture = Expect<ZCapture>(Visit(node.Capture));

        ZValue start = Visit(node.Start);
        ZValue end = Visit(node.End);
        ZValue counter = start.Copy();

        bool startLessThanEnd = start.IsLessOrEqualTo(end).IsTrue();

        ZValue stepValue = node.Step is not null ? Visit(node.Step!) : new ZInt(1);

        bool DoContinue() => (stepValue.IsGreaterOrEqualTo(new ZInt(0)).IsTrue() ? counter.IsLessOrEqualTo(end) : counter.IsGreaterOrEqualTo(end)).IsTrue();

        while (DoContinue())
        {
            capture.Set(counter);
            VisitResult result = Visit(node.Body);
            if (result.Flow == ExecutionFlow.Break) break;
            else if (result.Flow != ExecutionFlow.Normal && result.Flow != ExecutionFlow.Continue) return result;

            counter = counter.AddTo(stepValue);
        }

        return new ZNull(node.Pos);
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

    // FUNC-RELATED

    public VisitResult VisitSymbolAccess(SymbolAccessNode node)
    {
        string identifier = (node.IdToken.Value as string)!;

        if (Memory.Definitions.TryGetValue(identifier, out var i))
        {
            return new ZInt(i);
        }
        
        ErrorHandler.RTError("undefined symbol", $"undefined symbol: '{identifier}'", node.Pos);
        return new ZNull(node.Pos);
    }

    public VisitResult VisitFuncOutputAccess(FuncOutputAccessNode node)
    {
        if (CallStack.Count < 1)
        {
            ErrorHandler.RTError("invalid output access", "cannot access function output outside of function call!", node.Pos);
        }

        return CallStack[^1].Output;
    }

    public VisitResult VisitArgumentAccess(ArgumentAccessNode node)
    {
        if (CallStack.Count < 1)
        {
            ErrorHandler.RTError("invalid argument access", "cannot access function argument outside of function call!", node.Pos);
        }

        ZInt index = Expect<ZInt>(value: Visit(node.Index));
        return CallStack[^1].GetArgument(index.Value, node.Pos);
    }

    public VisitResult VisitReturn(ReturnNode node)
    {
        if (CallStack.Count < 1)
        {
            ErrorHandler.RTError("invalid return", "cannot return outside of function", node.Pos);
        }


        if (node.Value is not null)
        {
            ZValue returnValue = Visit(node.Value!);
            CallStack[^1].Output.Set(returnValue);
        }

        return new VisitResult(new ZNull(node.Pos), ExecutionFlow.Return);
    }

    public VisitResult VisitCall(CallNode node)
    {
        ZInt address = Expect<ZInt>(Visit(node.Address));
        ZCapture output = Expect<ZCapture>(Visit(node.Output));
        
        List<ZValue> argumentList = [];
        foreach (ASTNode argumentNode in node.Arguments)
        {
            argumentList.Add(Visit(argumentNode));
        }

        ZValue[] arguments = [..argumentList];

        ZFunctionDefinition func = Expect<ZFunctionDefinition>(Memory.GetExternal(address.Value, node.Pos));

        if (arguments.Length != func.ArgCount)
        {
            ErrorHandler.RTError("invalid call", $"expected {func.ArgCount} arguments, got {arguments.Length}", node.Pos);
        }

        CallStack.Add(new(arguments, output, this));
        Visit(func.Body);
        CallStack.RemoveAt(CallStack.Count - 1);

        return new ZNull(node.Pos);
    }

    public VisitResult VisitFuncDef(FuncDefNode node)
    {
        string identifier = (node.IdentifierToken.Value as string)!;
        int argumentCount = (node.ArgCountToken is not null ? node.ArgCountToken.Value as int? : null) ?? 0;

        int address = Memory.AddressCounter++;

        Memory.SetExternal(address, new ZFunctionDefinition(identifier, argumentCount, node.Body){ Pos = node.Pos }, node.Pos);
        Memory.Definitions[identifier] = address;

        return new ZInt(address);
    }

    // ARRAY-RELATED

    public VisitResult VisitReserveArray(ReserveArrayNode node)
    {
        ZInt elementCount = Expect<ZInt>(Visit(node.ElementCount));

        ZArray array = new(elementCount.Value);
        int address = Memory.AddressCounter++;

        Memory.SetExternal(address, array, node.Pos);
        return new ZInt(address);
    }

    public VisitResult VisitArray(ArrayNode node)
    {
        List<ZValue> elements = [];

        foreach (ASTNode elementNode in node.Elements)
        {
            elements.Add(Visit(elementNode));
        }

        ZArray array = new([.. elements]);
        int address = Memory.AddressCounter++;
        
        Memory.SetExternal(address, array, node.Pos);
        return new ZInt(address);
    }


}