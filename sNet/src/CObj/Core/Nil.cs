namespace sNet.CScriptPro;

public sealed class Nil : Obj
{
	private static readonly Lazy<Nil> Lazy = new Lazy<Nil>(() => new Nil());
	
	public static Nil Value => Lazy.Value;

	public override TypeId TypeId => TypeId.Nil;

	public override string ToString()
	{
		return "nil";
	}

	public override bool AsBool()
	{
		return false;
	}

	public override Obj Clone()
	{
		return Value;
	}
}