using System.Formats.Asn1;

namespace zyte;



class Parser(Token[] tokens, string filename)
{
    public Token[] Tokens = tokens;
    public Position Pos = new(filename);
    public Token Current { get => Pos.Index < Tokens.Length ? Tokens[Pos.Index] : Tokens.Last(); }
    public bool IsEnd { get => Current.Type == TokenType.End; }

    public void Next()
    {
        Pos.Next(Current.Type == TokenType.Newline);
    }

    public void Eat(TokenType tt, Position site)
    {
        if (Current.Type != tt)
        {
            ErrorHandler.SyntaxError("invalid syntax", $"expected {tt.ToString().ToLower()}", site);
        }

        Next();
    }
    
    public void Eat(TokenType tt, object value, Position site)
    {
        if (Current.Type == tt && Current.Value == value)
        {
            Next();
            return;
        }

        ErrorHandler.SyntaxError("invalid syntax", $"expected {tt.ToString().ToLower()} '{value}'", site);
    }
    

    public void EatNewlines()
    {
        while (Current.Type == TokenType.Newline && !IsEnd)
        {
            Next();
        }
    }


    public ASTNode Parse()
    {
        return Body(TokenType.End);
    }

    public ASTNode Body(TokenType stopAt)
    {
        List<ASTNode> tree = [];
        if (stopAt == TokenType.RightBrace && Current.Type == TokenType.LeftBrace) Next();

        while (Current.Type != stopAt && !IsEnd)
        {
            if (Current.Type == TokenType.Newline)
            {
                Next();
            }
            
            tree.Add(item: Controls());
        }

        Next();

        return new BodyNode([..tree]);
    }

    // PARSING METHODS

    public ASTNode Factor()
    {
        Token token = Current;
        Position start = token.Pos;

        if (token.Type == TokenType.Integer)
        {
            Next();
            return new IntegerNode(token)
            {
                Pos = start
            };
        }

        else if (token.Type == TokenType.String)
        {
            Next();
            return new StringNode(token)
            {
                Pos = start
            };
        }

        else if (token.Type == TokenType.Mul)
        {
            Next();
            ASTNode index = Factor();
            return new RegisterAccessNode(index)
            {
                Pos = start
            };
        }

        else if (token.Type == TokenType.Plus || token.Type == TokenType.Minus)
        {
            Next();
            ASTNode value = Factor();
            return new UnaryOperNode(token, value)
            {
                Pos = start
            };
        }

        else if (token.Type == TokenType.LeftParen)
        {
            Next();
            ASTNode expr = Expr();

            if (Current.Type != TokenType.RightParen)
            {
                ErrorHandler.SyntaxError("invalid expression", "expected ending ')', didn't find.", token.Pos);   
            }

            Next();
            return expr;
        }

        else if (token.Type == TokenType.Increment || token.Type == TokenType.Decrement)
        {
            Next();
            ASTNode value = Factor();

            return new ChangeValueNode(token, value)
            {
                Pos = start
            }; 
        }

        else if (token.IsKeyword("not"))
        {
            Next();
            ASTNode value = Factor();

            return new NotNode(value)
            {
                Pos = start
            };
        }
        
        return new NoNode()
        {
            Pos = start
        };
    }

    public ASTNode Term()
    {
        Token token = Current;
        Position start = token.Pos;

        if (token.Type == TokenType.Dot)
        {
            Next();
            ASTNode index = Factor();
            return new RegisterNode(index)
            {
                Pos = start
            };
        }

        else if (token.IsKeyword("_"))
        {
            Next();
            return new DiscardCaptureNode()
            {
                Pos = start
            };
        }

        return Factor();
    }

    public ASTNode Comp()
    {
        Token token = Current;
        Position start = token.Pos;

        if (new string[]{ "lt", "gt", "eq" }.Contains(token.Value))
        {
            ASTNode binOp = BinaryOper();
            binOp.Pos = start;
            return binOp;
        }

        return Term();
    }

    public ASTNode BinaryOper()
    {
        // The operation is expected to start at the operator
        Token operToken = Current;

        Next();
        ASTNode left = Expr();
        
        Eat(TokenType.Comma, Current.Pos);
        ASTNode right = Expr();

        return new BinaryOperNode(operToken, left, right);
    }

    public ASTNode Keyword()
    {
        Token token = Current;
        Position start = token.Pos;

        if (token.IsKeyword("cpy"))
        {
            Next();
            ASTNode register = Term();

            Eat(TokenType.Comma, start);
            ASTNode value = Expr();

            return new CopyValueNode(register, value)
            {
                Pos = start
            };
        }

        else if (token.IsKeyword("wln"))
        {
            Next();
            ASTNode value = Expr();

            return new PrintNode(value, true)
            {
                Pos = start
            };
        }

        else if (token.IsKeyword("w"))
        {
            Next();
            ASTNode value = Expr();

            return new PrintNode(value, false)
            {
                Pos = start
            };
        }

        else if (token.IsKeyword("rei"))
        {
            Next();
            ASTNode output = Term();

            return new ReadIntegerNode(output)
            {
                Pos = start
            };
        }

        else if (token.IsKeyword("rech"))
        {
            Next();
            ASTNode output = Term();

            return new ReadCharNode(output)
            {
                Pos = start
            };
        }

        else if (token.IsKeyword("rek"))
        {
            Next();
            ASTNode output = Term();

            return new ReadKeyNode(output)
            {
                Pos = start
            };
        }

        else if (token.IsKeyword("break"))
        {
            Next();

            return new BreakNode()
            {
                Pos = start
            };
        }

        else if (token.IsKeyword("next"))
        {
            Next();

            return new NextIterationNode()
            {
                Pos = start
            };
        }

        // Ternary condition
        else if (token.IsKeyword("if"))
        {
            Next();
            ASTNode condition = Expr();
            if (!Current.IsKeyword("then"))
            {
                ErrorHandler.SyntaxError("invalid syntax", $"expected 'then'", Current.Pos);
            }

            Next();
            ASTNode happyCase = Expr();
            
            if (!Current.IsKeyword("else"))
            {
                ErrorHandler.SyntaxError("invalid syntax", $"expected 'else'", Current.Pos);
            }

            Next();
            ASTNode badCase = Expr();

            return new TernaryIfNode(happyCase, badCase, condition)
            {
                Pos = start
            };
        }


        // LOGICAL EXPRESSIONS

        else if (token.IsKeyword("and"))
        {
            ASTNode binOp = BinaryOper();
            binOp.Pos = start;
            return binOp;
        }

        else if (token.IsKeyword("or"))
        {
            ASTNode binOp = BinaryOper();
            binOp.Pos = start;
            return binOp;
        }

        else if (token.IsKeyword("xor"))
        {
            ASTNode binOp = BinaryOper();
            binOp.Pos = start;
            return binOp;
        }

        return Comp();
    }

    public ASTNode Expr()
    {
        Token token = Current;
        Position start = token.Pos;

        if (token.Type == TokenType.LeftBrace)
        {
            Next();

            return Body(TokenType.RightBrace);
        }

        return Keyword();
    }

    public ASTNode Controls()
    {
        Token token = Current;
        Position start = token.Pos;


        if (token.IsKeyword("if"))
        {
            Next();
            List<IfCase> cases = [];

            // Head case
            ASTNode condition = Expr();
            EatNewlines();

            ASTNode body = Expr();
            cases.Add(new(condition, body)
            {
                Pos = start
            });

            // else if cases

            EatNewlines();
            while (Current.IsKeyword("elseif"))
            {
                Position elseIfStart = Current.Pos;

                Next();
                ASTNode elseIfCondition = Expr();

                EatNewlines();


                ASTNode elseIfBody = Expr();

                cases.Add(new(elseIfCondition, elseIfBody)
                {
                    Pos = elseIfStart
                });

                EatNewlines();
            }

            // else case
            
            IfCase? elseCase = null;
            if (Current.IsKeyword("else"))
            {
                Next();
                EatNewlines();

                ASTNode elseBody = Expr();

                elseCase = new(condition, elseBody);
            }

            return new IfNode([..cases], elseCase)
            {
                Pos = start
            };
        }



        // While node
        else if (token.IsKeyword("while"))
        {
            Next();

            ASTNode condition = Expr();
            EatNewlines();

            ASTNode body = Expr();

            return new WhileNode(condition, body)
            {
                Pos = start
            };
        }

        // For node
        else if (token.IsKeyword("for"))
        {
            Next();

            ASTNode captureNode = Expr();

            Eat(TokenType.Equals, Current.Pos);

            ASTNode startNode = Expr();

            if (!Current.IsKeyword("to"))
            {
                ErrorHandler.SyntaxError("invalid for-loop syntax", "expected 'to' after start expression", Current.Pos);    
            }
            
            Next();
            ASTNode endNode = Expr();
            ASTNode? stepNode = null;

            if (Current.IsKeyword("step"))
            {
                Next();
                stepNode = Expr();
            }

            ASTNode body = Expr();

            return new ForNode(captureNode, startNode, endNode, body, stepNode){
                Pos = start
            };
        }

        return Expr();
    }
}