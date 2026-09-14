namespace zyte;



class InterpretationContext(BodyNode bodyNode)
{
    public BodyNode Body = bodyNode;
    public virtual bool Running { get; set; } = true;
    public virtual bool ShouldContinue { get => Running; }
}


class LoopContext(BodyNode bodyNode, bool running) : InterpretationContext(bodyNode)
{
    public override bool Running { get; set; } = running;
    public override bool ShouldContinue { get => Running && !Continue; }
    public bool Continue = false;
}

class WhileLoopContext(BodyNode bodyNode, bool running) : LoopContext(bodyNode, running)
{
}