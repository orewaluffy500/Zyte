namespace zyte;

abstract class ASTNode
{
    public Position Pos = new("");
    public abstract override string ToString();
}



class BodyNode(ASTNode[] tree) : ASTNode
{
    public ASTNode[] Tree = [ .. tree.Where(x => x is not NoNode) ];
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

class ChangeValueNode(Token operToken, ASTNode value) : ASTNode
{
    public Token OperToken = operToken;
    public ASTNode Value = value;

    public override string ToString()
    {
        return $"(CHANGE {Value} USING {OperToken})";
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

class DiscardCaptureNode : ASTNode
{
    public override string ToString()
    {
        return $"DISCARD()";
    }
}



class DereferenceNode(ASTNode index) : ASTNode
{
    public ASTNode Index = index;

    public override string ToString()
    {
        return $"WHAT({Index})";
    }
}

class DereferenceExtNode(ASTNode address) : ASTNode
{
    public ASTNode Address = address;

    public override string ToString()
    {
        return $"EXT({Address})";
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



class BinaryOperNode(Token operToken, ASTNode left, ASTNode right) : ASTNode
{
    public Token OperToken = operToken;
    public ASTNode Left = left;
    public ASTNode Right = right;

    public override string ToString()
    {
        return $"({Left} {OperToken} {Right})";
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




// FLOW RELATED

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




// LOOP RELATED

class WhileNode(ASTNode condition, ASTNode body) : ASTNode
{
    public ASTNode Condition = condition;
    public ASTNode Body = body;

    public override string ToString()
    {
        return $"(WHILE {Condition} DO {Body})";
    }
}

class ForNode(ASTNode capture, ASTNode start, ASTNode end, ASTNode body, ASTNode? step) : ASTNode
{
    public ASTNode Capture = capture;
    public ASTNode Start = start;
    public ASTNode End = end;
    public ASTNode Body = body;
    public ASTNode? Step = step;

    public override string ToString()
    {
        return $"(FOR {Capture} = {Start} to {End} step {Step} do {Body})";
    }
}



class BreakNode : ASTNode
{
    public override string ToString()
    {
        return "(BREAK)";
    }
}

class NextIterationNode : ASTNode
{
    public override string ToString()
    {
        return "(NEXT)";
    }
}

// LOGICAL EXPRESSIONS

class NotNode(ASTNode value) : ASTNode
{
    public ASTNode Value = value;

    public override string ToString()
    {
        return $"!{Value}";
    }
}

// FUNCTION RELATED

class SymbolAccessNode(Token identifierToken) : ASTNode
{
    public Token IdToken = identifierToken;

    public override string ToString()
    {
        return $"(ACCESS {IdToken})";
    }
}

class FuncOutputAccessNode : ASTNode
{
    public override string ToString()
    {
        return $"(FUNC_RESULT)";
    }
}

class ArgumentAccessNode(ASTNode index) : ASTNode
{
    public ASTNode Index = index;

    public override string ToString()
    {
        return $"ARG({Index})";
    }
}

class FuncDefNode(Token identToken, Token? argCountToken, ASTNode body) : ASTNode
{
    public Token IdentifierToken = identToken;
    public Token? ArgCountToken = argCountToken;
    public ASTNode Body = body;

    public override string ToString()
    {
        return $"(FUNC {IdentifierToken} TAKES {ArgCountToken} ARGS, DOES {Body})";
    }
}

class CallNode(ASTNode addressNode, ASTNode[] arguments, ASTNode outputNode) : ASTNode
{
    public ASTNode Address = addressNode;
    public ASTNode Output = outputNode;
    public ASTNode[] Arguments = arguments;
    public override string ToString()
    {
        return $"(CALL {Address}( {string.Join(", ", Arguments)} ) RESULT GOES INTO {Output})";
    }
}

class ReturnNode(ASTNode? value) : ASTNode
{
    public ASTNode? Value = value;

    public override string ToString()
    {
        return $"(RETURN {Value})";
    }
}

// ARRAY-RELATED

class ReserveArrayNode(ASTNode elementCount) : ASTNode
{
    public ASTNode ElementCount = elementCount;

    public override string ToString()
    {
        return $"(ARRAY[{ElementCount}])";
    }
}

class ArrayNode(ASTNode[] elements) : ASTNode
{
    public ASTNode[] Elements = elements;

    public override string ToString()
    {
        return $"(ARRAY[{Elements.Length}] = {{{string.Join(", ", Elements)}}})";
    }
}

class FieldAccessNode(ASTNode addressNode, ASTNode fieldNode) : ASTNode
{
    public ASTNode Address = addressNode;
    public ASTNode Field = fieldNode;

    public override string ToString()
    {
        return $"(FIELD {Field} OF {Address})";
    }
}

class FieldAssignNode(ASTNode addressNode, ASTNode fieldNode, ASTNode valueNode) : ASTNode
{
    public ASTNode Address = addressNode;
    public ASTNode Field = fieldNode;
    public ASTNode Value = valueNode;

    public override string ToString()
    {
        return $"(FIELD {Field} OF {Address} = {Value})";
    }
}
