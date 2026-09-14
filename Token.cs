namespace zyte;


enum TokenType {
    String, Identifier, Keyword, Integer,
    Plus, Minus, Mul, Div, Dot, Octal, Colon, Comma,
    Increment, Decrement, ShiftLeft, ShiftRight,
    LeftParen, RightParen, LeftBrace, RightBrace,
    Newline, End
}


class Token(TokenType type, Position pos, object? value = null)
{
    public TokenType Type = type;
    public Position Pos = pos;
    public object? Value = value;

    public static Token End(Position pos) => new(TokenType.End, pos);

    public bool IsKeyword(string kw) => Type == TokenType.Keyword && Value as string == kw;

    public override string ToString()
    {
        return Value is null ? $"({Type})" : $"({Type}:{Value})";
    }
}