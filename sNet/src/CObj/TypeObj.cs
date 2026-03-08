namespace sNet.CScriptPro;

public sealed class TypeObj : Obj
{
	public TypeObj(TypeId id)
	{
		Id = id;
	}

	public override TypeId TypeId => TypeId.Nil;

	public TypeId Id { get; }
}