namespace zyte;


class ZStandard
{
    public Interpreter Interpreter;
    public string BuilderString = "";

    public ZStandard(Interpreter interpreter)
    {
        Interpreter = interpreter;
        AddFunction(StdLen, 1, "len");
        AddFunction(StdStrcat, 1, "strcat");
        AddFunction(StdStrflush, 0, "strflush");
    }

    public void AddFunction(Action<ZValue[], ZCapture, Position> body, int argCount, string id)
    {
        int address = Interpreter.Memory.AddressCounter++;
        Interpreter.Memory.SetExternal(address, new ZBuiltinFunc(body, argCount), new("<stdlib>"));
        Interpreter.Memory.Definitions[id] = address;
    }

    public T Expect<T>(ZValue value, Position site) where T : ZValue
    {
        if (value is T v) return v;

        string got = ZValue.GetLabelOf(value.GetType());
        string expected = ZValue.GetLabelOf(typeof(T));
        ErrorHandler.RTError("standard library type mismatch", $"expected {expected}, got {got}", site);
        return value.To<T>();
    }

    // FUNCTIONS

    public void StdLen(ZValue[] args, ZCapture output, Position site)
    {
        ZInt addr = Expect<ZInt>(args[0], site);
        ZArray array = Expect<ZArray>(Interpreter.Memory.GetExternal(addr.Value, site), site);

        output.Set(new ZInt(array.Elements.Count){ Pos = site });
    }

    public void StdStrcat(ZValue[] args, ZCapture output, Position site)
    {
        ZValue text = args[0];
        BuilderString += text is ZString s ? s.Value : text.ToString();
    }

    public void StdStrflush(ZValue[] args, ZCapture output, Position site)
    {
        ZString flushedString = new(BuilderString){ Pos = site };
        BuilderString = "";

        Interpreter.Memory.ExternalMemory[++Interpreter.Memory.AddressCounter] = flushedString;
        output.Set(new ZInt(Interpreter.Memory.AddressCounter){ Pos = site });
    }
}