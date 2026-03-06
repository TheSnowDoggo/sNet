namespace sNet.CScriptPro;

public sealed class MemberRef : CRef
{
	private readonly CObj _source;
	private readonly CObj _member;
	
	public MemberRef(CObj source, CObj member)
	{
		_source = source;
		_member = member;
	}

	public override CObj Value
	{
		get => _source[_member];
		set => _source[_member] = value;
	}
}