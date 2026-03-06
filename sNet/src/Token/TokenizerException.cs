namespace sNet.CScriptPro;

public sealed class TokenizerException : Exception
{
	public TokenizerException(int line, string message)
		: base($"[Tokenizer] Error at line {line}: {message}")
	{
	}
}