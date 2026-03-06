namespace sNet.CScriptPro;

public sealed class Nil : Obj
{
	private static readonly Lazy<Nil> _lazy = new Lazy<Nil>(() => new Nil());
	
	private Nil() : base(TypeId.Nil) { }
	
	public static Nil Value => _lazy.Value;

	public override string ToString()
	{
		return "nil";
	}

	public override Obj Clone()
	{
		return Value;
	}
}