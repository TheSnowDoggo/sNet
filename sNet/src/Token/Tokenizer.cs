using System.Text;

namespace sNet.CScriptPro;

public abstract class Tokenizer<Id, TokenType>
	where Id : struct
	where TokenType : Token<Id>
{
	private readonly TextReader _reader;
	private int _line;
	private char _current;

	public Tokenizer(TextReader reader)
	{
		_reader = reader;
	}
	
	protected virtual IReadOnlyDictionary<string, Id> DoubleMap => null;
	protected virtual IReadOnlyDictionary<char, Id> SingleMap => null;

	protected virtual IReadOnlySet<Id> CompoundSet => null;
	
	protected virtual IReadOnlyDictionary<string, Obj> LiteralMap => null;
	protected virtual IReadOnlyDictionary<string, Id> KeywordsMap => null;

	protected virtual Id IdentifierId => default;
	protected virtual Id LiteralId => default;

	protected abstract TokenType Create(int line, Id id, string lexeme, Obj value = null);

	public List<TokenType> Tokenize()
	{
		var tokens = new List<TokenType>();

		_line = 1;

		while (TryRead())
		{
			switch (_current)
			{
			case <= ' ':
				continue;
			case '/' when Peek() == '/':
				SingleComment();
				continue;
			case '/' when Peek() == '*':
				MultiComment();
				continue;
			}

			var token = NextToken();

			if (CompoundSet != null && CompoundSet.Contains(token.Type) && Peek() == '=')
			{
				TryRead();
				token.Lexeme += '=';
				token.Value = Bool.True;
			}
			
			tokens.Add(token);
		}

		return tokens;
	}
	
	private static bool IsDigitStart(char c)
	{
		return c is >= '0' and <= '9' or '-';
	}

	private static bool IsNumeric(char c)
	{
		return c is >= '0' and <= '9' or '.';
	}
	
	private static bool IsAlpha(char c)
	{
		return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
	}

	private static bool IsAlphaNumeric(char c)
	{
		return IsAlpha(c) || c is >= '0' and <= '9';
	}

	private static bool IsStringDelimiter(char c)
	{
		return c is '\"' or '\'';
	}
	
	private static char GetEscape(char c) => c switch
	{
		'0' => '\0',
		'a' => '\a',
		'b' => '\b',
		'f' => '\f',
		'n' => '\n',
		'r' => '\r',
		't' => '\t',
		'v' => '\v',
		'e' => '\e',
		'\\' => '\\',
		'\"' => '\"',
		'\'' => '\'',
		_ => '_',
	};

	private static bool TryGetEscape(char c, out char escape)
	{
		escape = GetEscape(c);
		return escape != '_';
	}

	private void SingleComment()
	{
		while (TryPeek(out char c) && c != '\n')
		{
			TryRead();
		}

		TryRead();
	}

	private void MultiComment()
	{
		char last = _current;
		TryRead();

		while (TryPeek(out char c) && !(last == '*' && c == '/'))
		{
			last = c;
			TryRead();
		}
		
		TryRead();

		if (_current != '/')
		{
			throw new TokenizerException(_line, "Multi-line comment missing end delimiter */.");
		}
	}
	
	private TokenType NextToken()
	{
		if (DoubleMap != null && DoubleMap.TryGetValue($"{_current}{Peek()}", out var tokenType))
		{
			TryRead();
			return Create(_line, tokenType, $"{_current}{Peek()}");
		}
		
		if (SingleMap != null && SingleMap.TryGetValue(_current, out var singleType))
		{
			return Create(_line, singleType, _current.ToString());
		}

		if (IsAlpha(_current))
		{
			return NextAlpha();
		}

		if (IsStringDelimiter(_current))
		{
			return NextString();
		}

		if (IsDigitStart(_current))
		{
			return NextNumber();
		}
		
		throw new TokenizerException(_line, $"Unrecognised symbol: (\'{_current}\')");
	}

	private TokenType NextAlpha()
	{
		var sb = new StringBuilder();
		sb.Append(_current);

		while (TryPeek(out char c) && IsAlphaNumeric(c))
		{
			TryRead();
			sb.Append(c);
		}

		var str = sb.ToString();

		if (LiteralMap != null && LiteralMap.TryGetValue(str, out Obj value))
		{
			return Create(_line, LiteralId, str, value);
		}

		if (KeywordsMap != null && KeywordsMap.TryGetValue(str, out Id keywordId))
		{
			return Create(_line, keywordId, str);
		}

		return Create(_line, IdentifierId, str);
	}

	private TokenType NextString()
	{
		char delim = _current;
		
		var sb = new StringBuilder();

		while (TryPeek(out char c) && !IsStringDelimiter(c))
		{
			TryRead();
			
			if (c != '\\' || !TryGetEscape(Peek(), out char escape))
			{
				sb.Append(c);
				continue;
			}

			TryRead();
			sb.Append(escape);
		}

		if (Peek() != delim)
		{
			throw new TokenizerException(_line, $"String missing end delimiter (\'{delim}\') or has mismatching delimiters.");
		}
		
		TryRead();
		
		var str = sb.ToString();
		var lexeme = $"{delim}{str}{delim}";

		return Create(_line, LiteralId, lexeme, (StrObj)str);
	}

	private TokenType NextNumber()
	{
		var sb = new StringBuilder();
		sb.Append(_current);

		while (TryPeek(out char c) && IsNumeric(c))
		{
			TryRead();
			sb.Append(c);
		}
		
		string numStr = sb.ToString();

		if (!double.TryParse(numStr, out var value))
		{
			throw new TokenizerException(_line, $"Invalid number literal: (\'{numStr}\')");
		}
		
		return Create(_line, LiteralId, numStr, (Number)value);
	}

	private char Peek()
	{
		int value = _reader.Peek();
		return value == -1 ? '\0' : (char)value;
	}

	private bool TryPeek(out char c)
	{
		int value = _reader.Peek();

		if (value == -1)
		{
			c = '\0';
			return false;
		}

		c = (char)value;
		return true;
	}

	private bool TryRead()
	{
		int value = _reader.Read();

		if (value < 0)
		{
			return false;
		}
		
		_current = (char)value;

		if (_current == '\n')
		{
			_line++;
		}
		
		return true;
	}
}