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
    public Position Pos = new("<unknown>");
    public abstract override string ToString();
    public abstract ZValue Copy();

    public T To<T>() where T : ZValue
    {
        return (T) this;
    }

    public ZValue Repos(Position newPos)
    {
        Pos = newPos;
        return this;
    }


    /* ---- OPERATIONS ---- */

    public ZValue IllegalOperation(string name)
    {
        ErrorHandler.ValueError("illegal operation", $"illegal operation '{name}' on value of type {GetLabelOf(GetType())}", Pos);
        return Copy();
    }

    public virtual ZValue Negate()                      => IllegalOperation("negate");
    public virtual ZValue Positate()                    => IllegalOperation("positate");
    public virtual ZValue Increment()                   => IllegalOperation("increment");
    public virtual ZValue Decrement()                   => IllegalOperation("decrement");
    public virtual ZValue IsEqualTo(ZValue other)       => IllegalOperation("equals");
    public virtual ZValue IsLessThan(ZValue other)      => IllegalOperation("less than");
    public virtual ZValue IsGreaterThan(ZValue other)   => IllegalOperation("greater than");
    public virtual ZValue Notted()                      => IllegalOperation("not");
    public virtual ZValue Anded(ZValue other)           => ZInt.FromCondition(IsTrue() && other.IsTrue());
    public virtual ZValue Ored(ZValue other)            => ZInt.FromCondition(IsTrue() || other.IsTrue());
    public virtual ZValue ExclOred(ZValue other)        => ZInt.FromCondition(IsTrue() != other.IsTrue());
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
            Pos = Pos
        };
    }

    public static ZInt FromCondition(bool condition)
    {
        return new ZInt(condition ? 1 : 0);
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

    public override ZValue Increment()
    {
        return new ZInt(Value + 1);
    }

    public override ZValue Decrement()
    {
        return new ZInt(Value - 1);
    }

    public override ZValue IsEqualTo(ZValue other)
    {
        if (other is not ZInt i) return IllegalOperation("equals");
        return FromCondition(Value == i.Value);
    }

    public override ZValue IsLessThan(ZValue other)
    {
        if (other is not ZInt i) return IllegalOperation("less than");
        return FromCondition(Value < i.Value);
    }

    public override ZValue IsGreaterThan(ZValue other)
    {
        if (other is not ZInt i) return IllegalOperation("greater than");
        return FromCondition(Value > i.Value);
    }

    public override ZValue Notted()
    {
        return FromCondition(IsFalse());
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
            Pos = Pos
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
            Pos = Pos
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
        return new ZNull(Pos);
    }

    public override string ToString()
    {
        return "null";
    }
}