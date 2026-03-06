namespace sNet.CScriptPro;

public sealed class StrObj : Obj, 
	IEquatable<StrObj>, 
	IComparable<StrObj>
{
	private readonly string _value;
	
	public StrObj(string value)
		: base(TypeId.String)
	{
		_value = value;
	}
	
	public int Length => _value.Length;
	
	public static StrObj Empty { get; } = new StrObj(string.Empty);
	
	public static implicit operator StrObj(string value) => new StrObj(value);
	public static implicit operator string(StrObj value) => value._value;

	public static bool operator ==(StrObj a, StrObj b) => Equals(a, b);
	public static bool operator !=(StrObj a, StrObj b) => !Equals(a, b);

	public static bool operator <(StrObj a, StrObj b) => a.CompareTo(b) < 0;
	public static bool operator >(StrObj a, StrObj b) => a.CompareTo(b) > 0;
	
	public static bool operator <=(StrObj a, StrObj b) => a.CompareTo(b) <= 0;
	public static bool operator >=(StrObj a, StrObj b) => a.CompareTo(b) >= 0;
	public static StrObj operator +(StrObj a, StrObj b) => a._value + b._value;

	public override Obj this[Obj key] => key.TypeId switch
	{
		TypeId.String => GetMember((StrObj)key),
		TypeId.Number => GetIndex((Number)key),
		_ => Nil.Value,
	};

	private Obj GetMember(string member) => member switch
	{
		"length" => (Number)Length,
		"indexOf" => GlobalFunction.Create(IndexOf, TypeId.String),
		"contains" => GlobalFunction.Create(Contains, TypeId.String),
		"subStr" => GlobalFunction.Create(SubString, TypeId.Number, TypeId.Number),
		"toLower" => GlobalFunction.Create(_ => _value.ToLower()),
		"toUpper" => GlobalFunction.Create(_ => _value.ToUpper()),
		"replace" => GlobalFunction.Create(Replace, TypeId.String, TypeId.String),
		"reversed" => GlobalFunction.Create(Reverse),
		"isPalindrome" => GlobalFunction.Create(IsPalindrome),
		_ => Nil.Value,
	};

	private Obj GetIndex(Number key)
	{
		int index = (int)key;
		return index >= 0 && index < Length ? _value[index].ToString() : Nil.Value;
	}

	public int CompareTo(StrObj other)
	{
		return other is null ? 1 : string.Compare(_value, other._value, StringComparison.Ordinal);
	}

	public static bool Equals(StrObj a, StrObj b)
	{
		if (a is null || b is null)
		{
			return ReferenceEquals(a, b);
		}

		return a._value == b._value;
	}

	public bool Equals(StrObj other)
	{
		return other is not null && other._value == _value;
	}

	public override bool Equals(object obj)
	{
		return obj is StrObj other && Equals(other);
	}

	public override int GetHashCode()
	{
		return _value.GetHashCode();
	}

	public override string ToString()
	{
		return _value;
	}

	public override Obj Clone()
	{
		return new StrObj(_value);
	}

	public override Obj Cast(TypeId to) => to switch
	{
		TypeId.String => this,
		TypeId.Number => Number.Parse(_value),
		TypeId.Bool => Bool.Parse(_value),
		_ => Nil.Value,
	};

	private Number IndexOf(Obj[] args)
	{
		string str = (StrObj)args[0];
		return _value.IndexOf(str, StringComparison.Ordinal);
	}
	
	private Bool Contains(Obj[] args)
	{
		string str = (StrObj)args[0];
		return _value.Contains(str, StringComparison.Ordinal);
	}

	private StrObj SubString(Obj[] args)
	{
		return _value.Substring((int)args[0], (int)args[1]);
	}

	private StrObj Replace(Obj[] args)
	{
		return _value.Replace((string)args[0], (string)args[1]);
	}

	private StrObj Reverse(Obj[] args)
	{
		var arr = new char[_value.Length];

		for (int i = 0; i < _value.Length; i++)
		{
			arr[arr.Length - i - 1] = _value[i];
		}

		return new string(arr);
	}

	private Bool IsPalindrome(Obj[] args)
	{
		int end = _value.Length / 2;

		for (int i = 0; i < end; i++)
		{
			if (_value[i] != _value[_value.Length - i - 1])
			{
				return false;
			}
		}
		
		return true;
	}
}