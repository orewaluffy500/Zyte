namespace zyte;


class ZStandard
{
    public Interpreter Interpreter;

    public ZStandard(Interpreter interpreter)
    {
        Interpreter = interpreter;
        AddFunction(StdLen, 1, "len");
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
}