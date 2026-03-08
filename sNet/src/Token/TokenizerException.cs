namespace sNet.CScriptPro;

/// <summary>
/// Represents errors that occur while tokenizer execution.
/// </summary>
public sealed class TokenizerException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TokenizerException"/> class, using the current line number and message.
	/// </summary>
	/// <param name="line">The current line number.</param>
	/// <param name="message">The exception message.</param>
	public TokenizerException(int line, string message)
		: base($"[Tokenizer] Error at line {line}: {message}")
	{
	}
}