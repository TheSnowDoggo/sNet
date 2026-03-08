namespace sNet.CScriptPro;

public readonly struct Uid : IEquatable<Uid>
{
	private readonly long _value;

	public Uid(long value)
	{
		_value = value;
	}

	public static Uid Null => new Uid(0);

	public static implicit operator long(Uid value) => value._value;
	public static implicit operator Uid(long value) => new Uid(value);
	
	public static bool operator ==(Uid a, Uid b) => a.Equals(b);
	public static bool operator !=(Uid a, Uid b) => !a.Equals(b);
	
	public static Uid FromHex(string hex)
	{
		return Convert.ToInt64(hex, 16);
	}

	public bool Equals(Uid other)
	{
		return _value == other._value;
	}

	public override bool Equals(object obj)
	{
		return obj is Uid uid && Equals(uid);
	}

	public override int GetHashCode()
	{
		return _value.GetHashCode();
	}

	public override string ToString()
	{
		return Convert.ToString(_value, 16);
	}
}