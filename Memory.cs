namespace zyte;


class Memory
{
    public ZInt[] Registers;

    public Memory(int regCount)
    {
        Position position = new("<memory>");

        Registers = [.. Enumerable.Range(1, regCount).Select(_ => new ZInt(0){ Pos = position.Copy() })];
    }

    public int SanitizeIndex(int index)
    {
        if (index < 0) return 0;
        if (index >= Registers.Length) return Registers.Length - 1;
        return index;
    }

    public ZInt GetRegister(int index)
    {
        ZInt register = Registers[SanitizeIndex(index)];
        return new(register.Value)
        {
            Pos = register.Pos
        };
    }

    public void SetRegister(int index, ZInt value)
    {
        Registers[SanitizeIndex(index)].Value = value.Value;
    }

    public void SetRegister(int index, int value)
    {
        Registers[SanitizeIndex(index)].Value = value;
    }
}