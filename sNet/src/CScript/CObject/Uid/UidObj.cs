using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class UidObj : Obj, IEquatable<UidObj>
{
	public static readonly ReadOnlyTable Export = new Dictionary<Obj, Obj>()
	{
		{ "new", GlobalFunction.Create(New, TypeId.String) },
	}.ToFrozenDictionary();

	private readonly Uid _value;

	public UidObj(Uid value)
	{
		_value = value;
	}

	public override TypeId TypeId => TypeId.Uid;

	public static implicit operator Uid(UidObj value) => value._value;
	public static implicit operator UidObj(Uid value) => new UidObj(value);

	public static bool operator ==(UidObj a, UidObj b) => Equals(a, b);
	public static bool operator !=(UidObj a, UidObj b) => !Equals(a, b);

	private static UidObj New(Obj[] args)
	{
		return Uid.FromHex((string)args[0]);
	}

	public static bool Equals(UidObj a, UidObj b)
	{
		if (a is null || b is null)
		{
			return ReferenceEquals(a, b);
		}

		return a._value == b._value;
	}

	public bool Equals(UidObj other)
	{
		return other is not null && other._value == _value;
	}

	public override bool Equals(object obj)
	{
		return obj is UidObj uid && Equals(uid);
	}

	public override int GetHashCode()
	{
		return _value.GetHashCode();
	}
	
	public override string ToString()
	{
		return _value.ToString();
	}
}