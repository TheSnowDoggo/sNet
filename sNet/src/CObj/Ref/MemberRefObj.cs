namespace sNet.CScriptPro;

public sealed class MemberRefObj : RefObj
{
	private readonly Obj _source;
	private readonly Obj _member;
	
	public MemberRefObj(Obj source, Obj member)
	{
		_source = source;
		_member = member;
	}

	public override Obj Value
	{
		get => _source[_member];
		set => _source[_member] = value;
	}
}