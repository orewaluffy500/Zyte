namespace zyte;

enum ExecutionFlow
{
    Normal,
    Break,
    Continue,
    Return,
}



class VisitResult(ZValue value, ExecutionFlow flow = ExecutionFlow.Normal)
{
    public ZValue Value = value;
    public ExecutionFlow Flow = flow;

    public VisitResult Repos(Position pos)
    {
        Value = Value.Repos(pos);
        return this;
    }

    public static implicit operator VisitResult(ZValue value)
    {
        return new(value);
    }
    public static implicit operator ZValue(VisitResult r) => r.Value;
}