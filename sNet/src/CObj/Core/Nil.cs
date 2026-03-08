namespace sNet.CScriptPro;

public sealed class Nil : Obj
{
	private static readonly Lazy<Nil> _lazy = new Lazy<Nil>(() => new Nil());
	
	public static Nil Value => _lazy.Value;

	public override TypeId TypeId => TypeId.Nil;

	public override string ToString()
	{
		return "nil";
	}

	public override Obj Clone()
	{
		return Value;
	}
}