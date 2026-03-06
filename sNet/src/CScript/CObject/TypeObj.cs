namespace sNet.CScriptPro;

public sealed class TypeObj : Obj
{
	public TypeObj(TypeId id)
		: base(TypeId.Nil)
	{
		Id = id;
	}
	
	public TypeId Id { get; }
}