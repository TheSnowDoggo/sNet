using System.Runtime.CompilerServices;

namespace sNet.CScriptPro;

public abstract class CObj
{
	public CObj(TypeId typeId)
	{
		TypeId = typeId;
	}
	
	public TypeId TypeId { get; }

	public virtual CObj this[CObj key]
	{
		get => Nil.Value;
		set { }
	}

	public static implicit operator CObj(double value) => (Number)value;
	public static implicit operator CObj(string value) => (CStr)value;
	public static implicit operator CObj(bool value) => (Bool)value;
	public static implicit operator CObj(Vec2 value) => (CVec2)value;
	
	public static explicit operator double(CObj value) => (Number)value;
	public static explicit operator string(CObj value) => (CStr)value;
	public static explicit operator bool(CObj value) => (Bool)value;
	public static explicit operator Vec2(CObj value) => (CVec2)value;

	public static void Deref(ref CObj obj)
	{
		if (obj is CRef cref)
		{
			obj = cref.Value;
		}
	}

	public virtual bool AsBool()
	{
		return false;
	}

	public virtual CObj Cast(TypeId to)
	{
		return to == TypeId.String ? ToString() : Nil.Value;
	}

	public virtual CObj Clone()
	{
		return Nil.Value;
	}

	public CObj Expect(TypeId expectedId, [CallerMemberName] string memberName = "", [CallerFilePath] string filepath = "")
	{
		if (TypeId != expectedId)
		{
			throw new ArgumentException($"At {Path.GetFileNameWithoutExtension(filepath)}.{memberName}(), Expected {expectedId} but got {TypeId}");
		}

		return this;
	}
}