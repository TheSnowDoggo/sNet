using System.Runtime.CompilerServices;

namespace sNet.CScriptPro;

public abstract class Obj
{
	public Obj(TypeId typeId)
	{
		TypeId = typeId;
	}
	
	public TypeId TypeId { get; }

	public virtual Obj this[Obj key]
	{
		get => Nil.Value;
		set { }
	}

	public static implicit operator Obj(double value) => (Number)value;
	public static implicit operator Obj(string value) => (StrObj)value;
	public static implicit operator Obj(bool value) => (Bool)value;
	public static implicit operator Obj(Vector2 value) => (Vec2Obj)value;
	
	public static explicit operator double(Obj value) => (Number)value;
	public static explicit operator string(Obj value) => (StrObj)value;
	public static explicit operator bool(Obj value) => (Bool)value;
	public static explicit operator Vector2(Obj value) => (Vec2Obj)value;

	public static void Deref(ref Obj obj)
	{
		if (obj is RefObj cref)
		{
			obj = cref.Value;
		}
	}

	public virtual bool AsBool()
	{
		return false;
	}

	public virtual Obj Cast(TypeId to)
	{
		return to == TypeId.String ? ToString() : Nil.Value;
	}

	public virtual Obj Clone()
	{
		return Nil.Value;
	}

	public Obj Expect(TypeId expectedId, [CallerMemberName] string memberName = "", [CallerFilePath] string filepath = "")
	{
		if (TypeId != expectedId)
		{
			throw new ArgumentException($"At {Path.GetFileNameWithoutExtension(filepath)}.{memberName}(), Expected {expectedId} but got {TypeId}");
		}

		return this;
	}

	public T Expect<T>(TypeId expectedId, [CallerMemberName] string memberName = "", [CallerFilePath] string filepath = "")
		where T : Obj
	{
		return (T)Expect(expectedId, memberName, filepath);
	}
}