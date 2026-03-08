using System.Globalization;

namespace sNet.CScriptPro;

public sealed class EnumTable<TEnum> : Obj
	where TEnum : struct, IConvertible
{
	public override TypeId TypeId => TypeId.Enum;

	public override Obj this[Obj key]
	{
		get
		{
			if (key.TypeId != TypeId.String)
			{
				return Nil.Value;
			}

			if (Enum.TryParse((string)key, true, out TEnum result))
			{
				return result.ToInt32(null);
			}
			
			return Nil.Value;
		}
	}
}