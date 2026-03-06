namespace sNet.CScriptPro;

public sealed class CsrToken : Token<CsrId>
{
	public CsrToken(int line, CsrId type, string lexeme, Obj value)
		: base(line, type, lexeme, value) { }
	
	public bool IsCompound()
	{
		return CsrTokenizer.Compound.Contains(Type) && ReferenceEquals(Value, Bool.True);
	}
}