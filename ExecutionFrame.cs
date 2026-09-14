namespace zyte;



class ExecutionFrame(BodyNode body, ExecutionFrame? back = null)
{
    public ExecutionFrame? Back = back;
    public ASTNode[] Body = body.Tree;
    public int Index = 0;
    public ASTNode Current { get => Index < Body.Length ? Body[Index] : Body.Last(); }
    public bool IsTop { get => Back is null; }

    public ExecutionFrame GetLast()
    {
        if (Back is null) return this;

        return Back.GetLast();
    }
}