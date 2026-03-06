using System.Globalization;
using System.Runtime.CompilerServices;

namespace sNet.CScriptPro;

public sealed class Number : Obj,
	IEquatable<Number>,
	IComparable<Number>
{
	private readonly double _value;

	public Number(double value)
		: base(TypeId.Number)
	{
		_value = value;
	}
	
	public static implicit operator Number(double value) => new Number(value);
	public static implicit operator double(Number value) => value._value;
	
	public static bool operator ==(Number a, Number b) => Equals(a, b);
	public static bool operator !=(Number a, Number b) => !Equals(a, b);

	public static bool operator >(Number a, Number b) => a._value > b._value;
	public static bool operator <(Number a, Number b) => a._value < b._value;
	
	public static bool operator >=(Number a, Number b) => a._value >= b._value;
	public static bool operator <=(Number a, Number b) => a._value <= b._value;

	public static double operator *(Number a, Number b) => a._value * b._value;
	public static double operator /(Number a, Number b) => a._value / b._value;
	public static double operator %(Number a, Number b) => a._value % b._value;
	public static double operator +(Number a, Number b) => a._value + b._value;
	public static double operator -(Number a, Number b) => a._value - b._value;

	public static double operator <<(Number a, Number b) => (int)a._value << (int)b._value;
	public static double operator >>(Number a, Number b) => (int)a._value >> (int)b._value;

	public static double operator &(Number a, Number b) => (int)a._value & (int)b._value;
	public static double operator ^(Number a, Number b) => (int)a._value ^ (int)b._value;
	public static double operator |(Number a, Number b) => (int)a._value | (int)b._value;

	public static double operator -(Number a) => -a._value;
	public static double operator ~(Number a) => ~(int)a._value;

	public static bool Equals(Number a, Number b)
	{
		if (a is null || b is null)
		{
			return ReferenceEquals(a, b);
		}

		return a._value == b._value;
	}
	
	public static Obj Parse(string s)
	{
		return double.TryParse(s, out var result) ? result : Nil.Value;
	}

	public int CompareTo(Number other)
	{
		return other is null ? 1 : _value.CompareTo(other._value);
	}

	public bool Equals(Number other)
	{
		return other is not null && other._value.Equals(_value);
	}

	public override bool Equals(object obj)
	{
		return obj is Number number && Equals(number);
	}

	public override int GetHashCode()
	{
		return _value.GetHashCode();
	}

	public override string ToString()
	{
		return _value.ToString(CultureInfo.InvariantCulture);
	}

	public override bool AsBool()
	{
		return _value != 0;
	}

	public override Obj Clone()
	{
		return new Number(_value);
	}

	public override Obj Cast(TypeId to) => to switch
	{
		TypeId.String => ToString(),
		TypeId.Number => this,
		TypeId.Bool => new Bool(_value != 0),
		_ => Nil.Value,
	};
}