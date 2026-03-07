namespace sNet.CScriptPro;

public readonly struct TypeSet : IEquatable<TypeSet>
{
    public TypeSet(TypeId type1, TypeId type2)
    {
        Type1 = type1;
        Type2 = type2;
    }
    
    public TypeId Type1 { get; }
    public TypeId Type2 { get; }
    
    public static implicit operator TypeSet((TypeId, TypeId) tuple)
        => new  TypeSet(tuple.Item1, tuple.Item2);
    
    public static bool operator ==(TypeSet left, TypeSet right) => left.Equals(right);
    public static bool operator !=(TypeSet left, TypeSet right) => !left.Equals(right);

    public bool Equals(TypeSet other)
    {
        return other.Type1 == Type1 && other.Type2 == Type2;
    }

    public override bool Equals(object obj)
    {
        return obj is TypeSet other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type1, Type2);
    }

    public override string ToString()
    {
        return $"({Type1}, {Type2})";
    }
}