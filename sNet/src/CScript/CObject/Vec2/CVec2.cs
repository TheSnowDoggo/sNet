using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class CVec2 : CObj,
    IEquatable<CVec2>
{
    private readonly Vec2 _value;

    public static readonly ReadOnlyTable Export = new Dictionary<CObj, CObj>()
    {
        { "new", GlobalFunction.Create(New, TypeId.Number, TypeId.Number) },
        { "ZERO", new CVec2(Vec2.Zero) },
        { "ONE", new CVec2(Vec2.One) },
        { "UP", new CVec2(Vec2.Up) },
        { "DOWN", new CVec2(Vec2.Down) },
        { "LEFT", new CVec2(Vec2.Left) },
        { "RIGHT", new CVec2(Vec2.Right) },
    }.ToFrozenDictionary();
    
    public CVec2(Vec2 value)
        : base(TypeId.Vec2)
    {
        _value = value;
    }
    
    public CVec2(Number x, Number y)
        : this(new Vec2((double)x, (double)y))
    {
    }

    public static implicit operator CVec2(Vec2 v) => new CVec2(v);
    public static implicit operator Vec2(CVec2 v) => v._value;
    
    public static bool operator ==(CVec2 a, CVec2 b) => Equals(a, b);
    public static bool operator !=(CVec2 a, CVec2 b) => !Equals(a, b);

    public static Vec2 operator *(CVec2 a, CVec2 b) => a._value * b._value;
    public static Vec2 operator *(CVec2 v, Number x) => v._value * (double)x;
    
    public static Vec2 operator /(CVec2 a, CVec2 b) => a._value / b._value;
    public static Vec2 operator /(CVec2 v, Number x) => v._value / (double)x;
    
    public static Vec2 operator +(CVec2 a, CVec2 b) => a._value + b._value;
    public static Vec2 operator -(CVec2 a, CVec2 b) => a._value - b._value;

    public override CObj this[CObj key] => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
    {
        "x" => _value.X,
        "y" => _value.Y,
        "length" => _value.Length(),
        "lengthSquared" => _value.LengthSquared(),
        "floored" => CreateMember(_value.Floored),
        "truncated" => CreateMember(_value.Truncated),
        "ceiled" => CreateMember(_value.Ceiled),
        "rounded" => CreateMember(_value.Rounded),
        "normalized" => CreateMember(_value.Normalized),
        "dot" => GlobalFunction.Create(args => (Number)_value.Dot((CVec2)args[0]), TypeId.Vec2),
        "rotated90" => CreateMember(_value.Rotated90),
        "rotated180" => CreateMember(_value.Rotated180),
        "rotated270" => CreateMember(_value.Rotated270),
        "rotated" => GlobalFunction.Create(args => (CVec2)_value.Rotated((double)args[0]), TypeId.Number),
        _ => Nil.Value,
    };

    public static bool Equals(CVec2 a, CVec2 b)
    {
        if (a is null || b is null)
        {
            return ReferenceEquals(a, b);
        }

        return a._value == b._value;
    }

    public bool Equals(CVec2 other)
    {
        return other is not null && other._value == _value;
    }

    public override bool Equals(object obj)
    {
        return obj is CVec2 cVec2 && Equals(cVec2);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString();
    }
    
    private static CVec2 New(CObj[] args)
    {
        return new CVec2((Number)args[0].Cast(TypeId.Number), (Number)args[1].Cast(TypeId.Number));
    }

    private static GlobalFunction CreateMember(Func<Vec2> func)
    {
        return GlobalFunction.Create(Func);
        
        CVec2 Func(CObj[] _)
        {
            return func.Invoke();
        }
    }
}