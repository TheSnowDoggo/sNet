using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class Vec2Obj : Obj,
    IEquatable<Vec2Obj>
{
    private readonly Vector2 _value;

    public static readonly ReadOnlyTable Export = new Dictionary<Obj, Obj>()
    {
        { "new", GlobalFunction.Create(New, TypeId.Number, TypeId.Number) },
        { "ZERO", new Vec2Obj(Vector2.Zero) },
        { "ONE", new Vec2Obj(Vector2.One) },
        { "UP", new Vec2Obj(Vector2.Up) },
        { "DOWN", new Vec2Obj(Vector2.Down) },
        { "LEFT", new Vec2Obj(Vector2.Left) },
        { "RIGHT", new Vec2Obj(Vector2.Right) },
    }.ToFrozenDictionary();
    
    public Vec2Obj(Vector2 value)
    {
        _value = value;
    }
    
    public Vec2Obj(Number x, Number y)
    {
        _value = new Vector2((double)x, (double)y);
    }

    public override TypeId TypeId => TypeId.Vec2;

    public static implicit operator Vec2Obj(Vector2 v) => new Vec2Obj(v);
    public static implicit operator Vector2(Vec2Obj v) => v._value;
    
    public static bool operator ==(Vec2Obj a, Vec2Obj b) => Equals(a, b);
    public static bool operator !=(Vec2Obj a, Vec2Obj b) => !Equals(a, b);

    public static Vector2 operator *(Vec2Obj a, Vec2Obj b) => a._value * b._value;
    public static Vector2 operator *(Vec2Obj v, Number x) => v._value * (double)x;
    
    public static Vector2 operator /(Vec2Obj a, Vec2Obj b) => a._value / b._value;
    public static Vector2 operator /(Vec2Obj v, Number x) => v._value / (double)x;
    
    public static Vector2 operator +(Vec2Obj a, Vec2Obj b) => a._value + b._value;
    public static Vector2 operator -(Vec2Obj a, Vec2Obj b) => a._value - b._value;

    public static Vector2 operator -(Vec2Obj a) => -a._value;

    public override Obj this[Obj key] => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
    {
        "x" => _value.X,
        "y" => _value.Y,
        "length" => _value.Length(),
        "lengthSquared" => _value.LengthSquared(),
        "floored" => CreateMember(_value.Floored),
        "truncated" => CreateMember(_value.Truncated),
        "ceiled" => CreateMember(_value.Ceiled),
        "rounded" => CreateMember(_value.Rounded),
        "unit" => _value.Normalized(),
        "dot" => GlobalFunction.Create(args => (Number)_value.Dot((Vec2Obj)args[0]), TypeId.Vec2),
        "rotated90" => CreateMember(_value.Rotated90),
        "rotated180" => CreateMember(_value.Rotated180),
        "rotated270" => CreateMember(_value.Rotated270),
        "rotated" => GlobalFunction.Create(args => (Vec2Obj)_value.Rotated((double)args[0]), TypeId.Number),
        _ => Nil.Value,
    };

    public static bool Equals(Vec2Obj a, Vec2Obj b)
    {
        if (a is null || b is null)
        {
            return ReferenceEquals(a, b);
        }

        return a._value == b._value;
    }

    public bool Equals(Vec2Obj other)
    {
        return other is not null && other._value == _value;
    }

    public override bool Equals(object obj)
    {
        return obj is Vec2Obj cVec2 && Equals(cVec2);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString();
    }
    
    private static Vec2Obj New(Obj[] args)
    {
        return new Vec2Obj((Number)args[0].Cast(TypeId.Number), (Number)args[1].Cast(TypeId.Number));
    }

    private static GlobalFunction CreateMember(Func<Vector2> func)
    {
        return GlobalFunction.Create(Func);
        
        Vec2Obj Func(Obj[] _)
        {
            return func.Invoke();
        }
    }
}