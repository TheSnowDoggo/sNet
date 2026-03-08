namespace sNet.CScriptPro;

/// <summary>
/// Represents a base class for language tokens.
/// </summary>
/// <typeparam name="T">The token identifier type.</typeparam>
public abstract class Token<T>
	where T : struct
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Token{T}"/> class.
	/// </summary>
	/// <param name="line">The line number.</param>
	/// <param name="type">The token type.</param>
	/// <param name="lexeme">The source text of the token.</param>
	/// <param name="value">The token value.</param>
	protected Token(int line, T type, string lexeme, Obj value)
	{
		Line = line;
		Type = type;
		Lexeme = lexeme;
		Value = value;
	}

	/// <summary>
	/// Gets the line number of the token.
	/// </summary>
	public int Line { get; }
	
	/// <summary>
	/// Gets the token type.
	/// </summary>
	public T Type { get; set; }
	
	/// <summary>
	/// Gets the source text of the token.
	/// </summary>
	public string Lexeme { get; set; }
	
	/// <summary>
	/// Gets the token value.
	/// </summary>
	public Obj Value { get; set; }

	/// <inheritdoc/>
	public override string ToString()
	{
		return $"{Line} {Type} {Lexeme} {Value}";
	}
}