namespace sNet.CScriptPro;

public sealed class Bool : Obj, IEquatable<Bool>
{
	private static readonly Lazy<Bool> TrueLazy = new Lazy<Bool>(() => new Bool(true));
	public static readonly Lazy<Bool> FalseLazy = new Lazy<Bool>(() => new Bool(false));
	
	private readonly bool _value;

	private Bool(bool value)
	{
		_value = value;
	}

	public override TypeId TypeId => TypeId.Bool;

	public static Bool True => TrueLazy.Value;
	public static Bool False => FalseLazy.Value;
	
	public static implicit operator Bool(bool value) => value ? TrueLazy.Value : FalseLazy.Value;
	public static implicit operator bool(Bool value) => value._value;
	
	public static bool operator ==(Bool a, Bool b) => Equals(a, b);
	public static bool operator !=(Bool a, Bool b) => !Equals(a, b);

	public static bool operator &(Bool a, Bool b) => a._value & b._value;
	public static bool operator ^(Bool a, Bool b) => a._value ^ b._value;
	public static bool operator |(Bool a, Bool b) => a._value | b._value;
	
	public static bool operator false(Bool a) => !a._value;
	public static bool operator true(Bool a) => a._value;

	public static bool operator !(Bool a) => !a._value;

	public static Obj Parse(string s)
	{
		if (bool.TryParse(s, out var result))
		{
			return result;
		}

		return Nil.Value;
	}

	public static bool Equals(Bool a, Bool b)
	{
		if (a is null || b is null)
		{
			return ReferenceEquals(a, b);
		}

		return a._value == b._value;
	}
	
	public bool Equals(Bool other)
	{
		return other is not null && other._value == _value;
	}

	public override bool Equals(object obj)
	{
		return obj is Bool other && Equals(other);
	}

	public override int GetHashCode()
	{
		return _value.GetHashCode();
	}

	public override string ToString()
	{
		return _value ? "true" : "false";
	}

	public override bool AsBool()
	{
		return _value;
	}

	public override Obj Clone()
	{
		return _value;
	}

	public override Obj Cast(TypeId to) => to switch
	{
		TypeId.String => ToString(),
		TypeId.Number => _value ? 1 : 0,
		TypeId.Bool => this,
		_ => Nil.Value,
	};
}