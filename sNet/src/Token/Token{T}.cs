namespace sNet.CScriptPro;

public abstract class Token<T>
	where T : struct
{
	protected Token(int line, T type, string lexeme, CObj value)
	{
		Line = line;
		Type = type;
		Lexeme = lexeme;
		Value = value;
	}

	public int Line { get; }
	public T Type { get; set; }
	public string Lexeme { get; set; }
	public CObj Value { get; set; }

	public override string ToString()
	{
		return $"{Line} {Type} {Lexeme} {Value}";
	}
}