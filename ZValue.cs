namespace zyte;



abstract class ZValue
{
    public static Dictionary<string, string> Labels = new()
    {
        {nameof(ZValue), "any"},
        {nameof(ZInt), "integer"},
        {nameof(ZString), "string"},
        {nameof(ZCapture), "capture"},
        {nameof(ZDiscardCapture), "discard"},
        {nameof(ZNull), "null"},
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

    public virtual ZValue Negate()                              => IllegalOperation("negate");
    public virtual ZValue Positate()                            => IllegalOperation("positate");
    public virtual ZValue Increment()                           => IllegalOperation("increment");
    public virtual ZValue Decrement()                           => IllegalOperation("decrement");
    public virtual ZValue IsEqualTo(ZValue other)               => IllegalOperation("equals");
    public virtual ZValue IsLessThan(ZValue other)              => IllegalOperation("less than");
    public virtual ZValue IsGreaterThan(ZValue other)           => IllegalOperation("greater than");
    public virtual ZValue AddTo(ZValue other)                   => IllegalOperation("add");
    public virtual ZValue Notted()                              => IllegalOperation("not");
    public virtual ZValue IsGreaterOrEqualTo(ZValue other)      => ZInt.FromCondition(IsGreaterThan(other).IsTrue() || IsEqualTo(other).IsTrue());
    public virtual ZValue IsLessOrEqualTo(ZValue other)         => ZInt.FromCondition(IsLessThan(other).IsTrue() || IsEqualTo(other).IsTrue());
    public virtual ZValue Anded(ZValue other)                   => ZInt.FromCondition(IsTrue() && other.IsTrue());
    public virtual ZValue Ored(ZValue other)                    => ZInt.FromCondition(IsTrue() || other.IsTrue());
    public virtual ZValue ExclOred(ZValue other)                => ZInt.FromCondition(IsTrue() != other.IsTrue());
    public virtual bool IsTrue() => false;
    public virtual bool IsFalse() => !IsTrue();
}

abstract class ZCapture(Interpreter interpreter) : ZValue
{
    public Interpreter Interpreter = interpreter;

    public abstract void Set(ZValue value);
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

    public override ZValue AddTo(ZValue other)
    {
        if (other is not ZInt i) return IllegalOperation("add");
        return new ZInt(Value + i.Value);
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

class ZRegister(int index, Interpreter interpreter) : ZCapture(interpreter)
{
    public int Index = index;

    public override ZValue Copy()
    {
        return new ZRegister(Index, Interpreter);
    }

    public override void Set(ZValue value)
    {
        if (value is not ZInt)
        {
            ErrorHandler.RTError("type mismatch", "expected integer value to set register", Pos);
        }

        Interpreter.Memory.SetRegister(Index, (ZInt) value);
    }

    public override string ToString()
    {
        return $"<reg {Index}>";
    }
}

class ZDiscardCapture(Interpreter interpreter) : ZCapture(interpreter)
{
    public override ZValue Copy()
    {
        return new ZDiscardCapture(Interpreter);
    }

    public override void Set(ZValue value)
    {
    }

    public override string ToString()
    {
        return "DISCARD";
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

class ZArgumentCapture(CallFrame frame, int index, Interpreter interpreter) : ZCapture(interpreter)
{
    public CallFrame Frame = frame;
    public int Index = index;

    public override ZValue Copy()
    {
        return new ZArgumentCapture(Frame, Index, Interpreter);
    }

    public override void Set(ZValue value)
    {
        ZValue arg = Frame.Arguments[Index];
        if (arg is ZCapture c)
        {
            c.Set(value);
        }
    }

    public override string ToString()
    {
        return $"ARG({Index})";
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




class ZFunctionDefinition(string id, int argCount, ASTNode body) : ZValue
{
    public string Id = id;
    public int ArgCount = argCount;
    public ASTNode Body = body;

    public override ZValue Copy()
    {
        return new ZFunctionDefinition(Id, ArgCount, Body)
        {
            Pos = Pos
        };
    }

    public override string ToString()
    {
        return $"<func {Id} : {ArgCount}";
    }
}