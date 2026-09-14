namespace zyte;



abstract class ZValue
{
    public static Dictionary<string, string> Labels = new()
    {
        {nameof(ZValue), "any"},
        {nameof(ZInt), "integer"},
        {nameof(ZString), "string"},
        {nameof(ZNull), "null"}
    };

    public static string GetLabelOf(Type type) => Labels.TryGetValue(type.Name, out var s) ? s : "unknown";
    public Position Pos = new("");
    public abstract override string ToString();
    public abstract ZValue Copy();

    public T To<T>() where T : ZValue
    {
        return (T) this;
    }



    /* ---- OPERATIONS ---- */

    public virtual ZValue Negate() => Copy();
    public virtual ZValue Positate() => Copy();
    public virtual bool IsTrue() => false;
    public virtual bool IsFalse() => !IsTrue();
}


class ZInt(int value) : ZValue
{
    public int Value = value;

    public override ZValue Copy()
    {
        return new ZInt(Value)
        {
            Pos = Pos.Copy()
        };
    }

    public override string ToString()
    {
        return $"{Value}";
    }

    // OPERATIONS

    public override ZValue Negate()
    {
        return new ZInt(-Value);
    }

    public override ZValue Positate()
    {
        return new ZInt(+Value);
    }

    public override bool IsTrue()
    {
        return Value != 0;
    }
}

class ZRegister(int index) : ZValue
{
    public int Index = index;

    public override ZValue Copy()
    {
        return new ZRegister(Index)
        {
            Pos = Pos.Copy()
        };
    }

    public override string ToString()
    {
        return $"<reg .{Index}>";
    }
}

class ZString(string value) : ZValue
{
    public string Value = value;

    public override ZValue Copy()
    {
        return new ZString(Value)
        {
            Pos = Pos.Copy()
        };
    }

    public override string ToString()
    {
        return $"{Value}";
    }

    // Operations

    public override bool IsTrue()
    {
        return Value.Length > 0;
    }
}

class ZNull(Position pos) : ZValue
{
    public new Position Pos = pos;

    public override ZValue Copy()
    {
        return new ZNull(Pos.Copy());
    }

    public override string ToString()
    {
        return "null";
    }
}