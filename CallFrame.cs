namespace zyte;


class CallFrame(ZValue[] arguments, ZCapture output, Interpreter interpreter)
{
    public ZValue[] Arguments = arguments;
    public ZCapture Output = output;
    public Interpreter Interpreter = interpreter;

    public ZValue GetArgumentRef(int index, Position site)
    {
        if (index < 0 || index >= Arguments.Length)
        {
            ErrorHandler.RTError("invalid argument access", $"argument '{index}' is out-of-bounds", site);
        }

        return new ZArgumentCapture(this, index, Interpreter);
    }

    public ZValue GetArgument(int index, Position site)
    {
        if (index < 0 || index >= Arguments.Length)
        {
            ErrorHandler.RTError("invalid argument access", $"argument '{index}' is out-of-bounds", site);
        }

        return Arguments[index];
    }
}