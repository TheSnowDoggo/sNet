namespace sNet.CScriptPro;

public sealed class CType : CObj
{
	public CType(TypeId id)
		: base(TypeId.Nil)
	{
		Id = id;
	}
	
	public TypeId Id { get; }
}