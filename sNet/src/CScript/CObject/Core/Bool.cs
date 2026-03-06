namespace sNet.CScriptPro;

public sealed class Bool : Obj, IEquatable<Bool>
{
	private readonly bool _value;

	public Bool(bool value)
		: base(TypeId.Bool)
	{
		_value = value;
	}

	public static Bool True { get; } = new Bool(true);
	public static Bool False { get; } = new Bool(false);
	
	public static implicit operator Bool(bool value) => new Bool(value);
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
		return new Bool(_value);
	}

	public override Obj Cast(TypeId to) => to switch
	{
		TypeId.String => ToString(),
		TypeId.Number => _value ? 1 : 0,
		TypeId.Bool => this,
		_ => Nil.Value,
	};
}