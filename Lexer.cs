namespace zyte;


class Lexer(string source, string filename)
{
    public string Source = source;
    public Position Pos = new(filename);
    public char Current { get => Pos.Index < Source.Length ? Source[Pos.Index] : '\0'; }
    public bool IsEnd { get => Current == '\0'; }

    public void Next()
    {
        Pos.Next(Current == '\n');
    }

    public Token[] MakeTokens()
    {
        List<Token> tokens = [];

        while (!IsEnd)
        {
            if (" \t\v\r".Contains(Current))
            {
                Next();
            }

            else if (Current == '#')
            {
                while (!IsEnd && Current != '\n')
                {
                    Next();
                }
            }

            else if (Current == '+')
            {
                Position start = Pos.Copy();
                TokenType tokType = TokenType.Plus;
                Next();
                if (Current == '='){
                    tokType = TokenType.Increment;
                    Next();
                }

                tokens.Add(new(tokType, start));
            }

            else if (Current == '-')
            {
                Position start = Pos.Copy();
                TokenType tokType = TokenType.Minus;
                Next();
                if (Current == '='){
                    tokType = TokenType.Decrement;
                    Next();
                }

                tokens.Add(new(tokType, start));
            }

            else if (Current == '*')
            {
                tokens.Add(new(TokenType.Mul, Pos.Copy()));
                Next();
            }

            else if (Current == '/')
            {
                tokens.Add(new(TokenType.Div, Pos.Copy()));
                Next();
            }

            else if (Current == '.')
            {
                tokens.Add(new(TokenType.Dot, Pos.Copy()));
                Next();
            }

            else if (Current == ',')
            {
                tokens.Add(new(TokenType.Comma, Pos.Copy()));
                Next();
            }

            else if (Current == '\n')
            {
                tokens.Add(new(TokenType.Newline, Pos.Copy()));
                Next();
            }

            else if (Current == '#')
            {
                tokens.Add(new(TokenType.Octal, Pos.Copy()));
                Next();
            }

            else if (Current == ':')
            {
                tokens.Add(new(TokenType.Colon, Pos.Copy()));
                Next();
            }

            else if (Current == '(')
            {
                tokens.Add(new(TokenType.LeftParen, Pos.Copy()));
                Next();
            }

            else if (Current == ')')
            {
                tokens.Add(new(TokenType.RightParen, Pos.Copy()));
                Next();
            }

            else if (Current == '{')
            {
                tokens.Add(new(TokenType.LeftBrace, Pos.Copy()));
                Next();
            }

            else if (Current == '}')
            {
                tokens.Add(new(TokenType.RightBrace, Pos.Copy()));
                Next();
            }

            else if (char.IsLetter(Current) || Current == '_')
            {
                tokens.Add(MakeIdentifier());
            }

            else if (char.IsDigit(Current))
            {
                tokens.Add(MakeInteger());
            }

            else if (Current == '"')
            {
                tokens.Add(MakeString());
            }

            else
            {
                Next();
            }
        }

        tokens.Add(Token.End(Pos));
        return [ .. tokens ];   
    }

    public Token MakeIdentifier()
    {
        string identifier = "";
        Position start = Pos.Copy();

        while (!IsEnd && (char.IsLetterOrDigit(Current) || Current == '_'))
        {
            identifier += Current;
            Next();
        }

        return new(
            KeywordClass.Keywords.Contains(identifier) ? TokenType.Keyword : TokenType.Identifier, 
            start,
            identifier);
    }

    public Token MakeInteger()
    {
        string integer = "";
        Position start = Pos.Copy();

        while (!IsEnd && char.IsDigit(Current))
        {
            integer += Current;
            Next();
        }

        return new(TokenType.Integer, start, int.TryParse(integer, out int i) ? i : 0);
    }

    public Token MakeString()
    {
        string str = "";
        Position start = Pos.Copy();
        Next();

        while (Current != '"')
        {
            if (Current == '\n')
            {
                ErrorHandler.SyntaxError("invalid syntax", "unterminated string", Pos);
            }
            
            str += Current;
            Next();
        }

        Next();

        str = str.Replace("\\n", "\n");
        str = str.Replace("\\q", "\"");

        return new(TokenType.String, start, str);
    }
}