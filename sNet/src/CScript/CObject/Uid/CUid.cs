using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class CUid : CObj, IEquatable<CUid>
{
	public static readonly ReadOnlyTable Export = new Dictionary<CObj, CObj>()
	{
		{ "new", GlobalFunction.Create(New, TypeId.String) },
	}.ToFrozenDictionary();

	private readonly Uid _value;

	public CUid(Uid value)
		: base(TypeId.Uid)
	{
		_value = value;
	}

	public static implicit operator Uid(CUid value) => value._value;
	public static implicit operator CUid(Uid value) => new CUid(value);

	public static bool operator ==(CUid a, CUid b) => Equals(a, b);
	public static bool operator !=(CUid a, CUid b) => !Equals(a, b);

	private static CUid New(CObj[] args)
	{
		return Uid.FromHex((string)args[0]);
	}

	public static bool Equals(CUid a, CUid b)
	{
		if (a is null || b is null)
		{
			return ReferenceEquals(a, b);
		}

		return a._value == b._value;
	}

	public bool Equals(CUid other)
	{
		return other is not null && other._value == _value;
	}

	public override bool Equals(object obj)
	{
		return obj is CUid uid && Equals(uid);
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