namespace zyte;

abstract class ASTNode
{
    public Position Pos = new("");
    public abstract override string ToString();
}



class BodyNode(ASTNode[] tree) : ASTNode
{
    public ASTNode[] Tree = tree;
    public override string ToString()
    {
        return $"{{ {string.Join(' ', Tree)} }}";
    }
}


class NoNode : ASTNode
{
    public override string ToString()
    {
        return $"NONE";
    }
}

class EndNode : ASTNode
{
    public override string ToString()
    {
        return $"END";
    }
}


class IntegerNode(Token value) : ASTNode
{
    public Token Value = value;

    public override string ToString()
    {
        return $"{Value.Value}";
    }
}


class StringNode(Token value) : ASTNode
{
    public Token Value = value;

    public override string ToString()
    {
        return $"\"{Value.Value}\"";
    }
}


class RegisterNode(ASTNode index) : ASTNode
{
    public ASTNode Index = index;

    public override string ToString()
    {
        return $"REG({Index})";
    }
}


class RegisterAccessNode(ASTNode index) : ASTNode
{
    public ASTNode Index = index;

    public override string ToString()
    {
        return $"GETR({Index})";
    }
}

class CopyValueNode(ASTNode register, ASTNode value) : ASTNode
{
    public ASTNode Register = register;
    public ASTNode Value = value;

    public override string ToString()
    {
        return $"({Register} = {Value})";
    }
}


class PrintNode(ASTNode value, bool newline) : ASTNode
{
    public ASTNode Value = value;
    public bool Newline = newline;

    public override string ToString()
    {
        return Newline ? $"(PRINTLN {Value})" : $"(PRINT {Value})";
    }
}

class UnaryOperNode(Token opToken, ASTNode value) : ASTNode
{
    public Token OperToken = opToken;
    public ASTNode Value = value;

    public override string ToString()
    {
        return $"({OperToken} {Value})";
    }
}



/* INPUT NODES */

class ReadIntegerNode(ASTNode output) : ASTNode
{
    public ASTNode Output = output;

    public override string ToString()
    {
        return $"(integer input -> {Output})";
    }
}

class ReadCharNode(ASTNode output) : ASTNode
{
    public ASTNode Output = output;

    public override string ToString()
    {
        return $"(char input -> {Output})";
    }
}

class ReadKeyNode(ASTNode output) : ASTNode
{
    public ASTNode Output = output;

    public override string ToString()
    {
        return $"(key input -> {Output})";
    }
}



class IfCase(ASTNode condition, ASTNode body) : ASTNode
{
    public ASTNode Condition = condition;
    public ASTNode Body = body;

    public override string ToString()
    {
        return $"( IF ({Condition}) THEN {Body} )";
    }
}

class IfNode(IfCase[] cases, IfCase? elseCase = null) : ASTNode
{
    public IfCase[] Cases = cases;
    public IfCase? ElseCase = elseCase;

    public override string ToString()
    {
        return $"[ {string.Join(" or ", Cases)} else {ElseCase} ]";
    }
}


class TernaryIfNode(ASTNode happyCase, ASTNode badCase, ASTNode condition) : ASTNode
{
    public ASTNode HappyCase = happyCase;
    public ASTNode BadCase = badCase;
    public ASTNode Condition = condition;

    public override string ToString()
    {
        return $"({Condition} ? {HappyCase} : {BadCase})";
    }
}