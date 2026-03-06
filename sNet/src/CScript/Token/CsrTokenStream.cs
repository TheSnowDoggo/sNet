namespace sNet.CScriptPro;

public sealed class CsrTokenStream : TokenStream<CsrId, CsrToken>
{
	public CsrTokenStream(List<CsrToken> tokens, int start = 0)
		: base(tokens, start) { }
	
	public static implicit operator CsrTokenStream(List<CsrToken> tokens) => new CsrTokenStream(tokens);

	public CsrTokenStream SubStream()
	{
		return new CsrTokenStream(_tokens, Current);
	}
}