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

        tree.Add(new EndNode()
        {
            Pos = Pos.Copy()
        });

        Next();

        return new BodyNode([..tree]);
    }

    // PARSING METHODS

    public ASTNode Factor()
    {
        Token token = Current;
        Position start = token.Pos.Copy();

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

        return new NoNode();
    }

    public ASTNode Term()
    {
        Token token = Current;
        Position start = token.Pos.Copy();

        if (token.Type == TokenType.Dot)
        {
            Next();
            ASTNode index = Factor();
            return new RegisterNode(index)
            {
                Pos = start
            };
        }

        return Factor();
    }

    public ASTNode Keyword()
    {
        Token token = Current;
        Position start = token.Pos.Copy();

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

        // Ternary condition
        else if (token.IsKeyword("if"))
        {
            Next();
            ASTNode condition = Expr();
            if (!Current.IsKeyword("then"))
            {
                ErrorHandler.SyntaxError("invalid syntax", $"expected 'then'", Current.Pos.Copy());
            }

            Next();
            ASTNode happyCase = Expr();
            
            if (!Current.IsKeyword("else"))
            {
                ErrorHandler.SyntaxError("invalid syntax", $"expected 'else'", Current.Pos.Copy());
            }

            Next();
            ASTNode badCase = Expr();

            return new TernaryIfNode(happyCase, badCase, condition)
            {
                Pos = start
            };
        }

        return Term();
    }

    public ASTNode Expr()
    {
        Token token = Current;
        Position start = token.Pos.Copy();

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
        Position start = token.Pos.Copy();


        if (token.IsKeyword("if"))
        {
            Next();
            List<IfCase> cases = [];

            // Head case
            ASTNode condition = Expr();
            EatNewlines();
            Eat(TokenType.LeftBrace, Current.Pos.Copy());

            ASTNode body = Body(TokenType.RightBrace);
            cases.Add(new(condition, body)
            {
                Pos = start
            });

            // else if cases

            EatNewlines();
            while (Current.IsKeyword("elseif"))
            {
                Position elseIfStart = Current.Pos.Copy();

                Next();
                ASTNode elseIfCondition = Expr();

                EatNewlines();
                Eat(TokenType.LeftBrace, Current.Pos.Copy());

                ASTNode elseIfBody = Body(TokenType.RightBrace);

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
                Eat(TokenType.LeftBrace, Current.Pos.Copy());
                ASTNode elseBody = Body(TokenType.RightBrace);

                elseCase = new(condition, elseBody);
            }

            return new IfNode([..cases], elseCase)
            {
                Pos = start
            };
        }

        return Expr();
    }
}