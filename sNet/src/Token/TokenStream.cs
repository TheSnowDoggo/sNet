using System.Runtime.CompilerServices;

namespace sNet.CScriptPro;

public abstract class TokenStream<Id, TokenType>
    where Id : struct
    where TokenType : Token<Id>
{
    protected readonly List<TokenType> _tokens;
	
    public TokenStream(List<TokenType> tokens, int start = 0)
    {
        _tokens = tokens;
        Current = start;
    }
	
    public int Current { get; private set; }
    public int Line { get; private set; } = 1;

    public TokenType Peek()
    {
        return _tokens[Current];
    }

    public TokenType Read()
    {
        var tokens = _tokens[Current++];
		
        Line = tokens.Line;
		
        return tokens;
    }

    public TokenType LastLast()
    {
        return _tokens[Current - 2];
    }

    public TokenType Consume(Id expectedId, [CallerMemberName] string memberName = "")
    {
        if (EndOfStream())
        {
            throw new ParserException(Line, $"Expected token {expectedId}, ran out of tokens.", memberName);
        }
		
        var token = Read();

        if (!EqualityComparer<Id>.Default.Equals(token.Type, expectedId))
        {
            throw new ParserException(Line, $"Expected token {expectedId}, got {token.Type}.", memberName);
        }
		
        return token;
    }

    public bool EndOfStream()
    {
        return Current >= _tokens.Count;
    }
}