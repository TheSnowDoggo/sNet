using System.Runtime.CompilerServices;

namespace sNet.CScriptPro;

/// <summary>
/// Represents a base class for language token streams.
/// </summary>
/// <typeparam name="Id">The token id type.</typeparam>
/// <typeparam name="TokenType">The token type.</typeparam>
public abstract class TokenStream<Id, TokenType>
    where Id : struct
    where TokenType : Token<Id>
{
    protected readonly List<TokenType> _tokens;
	
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenStream{Id,TokenType}"/> class, from a list of tokens and start index.
    /// </summary>
    /// <param name="tokens">The list of tokens to read from.</param>
    /// <param name="start">The starting index.</param>
    public TokenStream(List<TokenType> tokens, int start = 0)
    {
        _tokens = tokens;
        Current = start;
    }
	
    /// <summary>
    /// Gets the current token index.
    /// </summary>
    public int Current { get; private set; }
    
    /// <summary>
    /// Gets the current line number.
    /// </summary>
    public int Line { get; private set; } = 1;

    /// <summary>
    /// Gets a value that indicates whether the current stream position is at the end of the stream.
    /// </summary>
    public bool EndOfStream => Current >= _tokens.Count;

    public int Length => _tokens.Count;

    /// <summary>
    /// Reads the next token from the token stream but does not consume it.
    /// </summary>
    /// <returns>The next token.</returns>
    public TokenType Peek()
    {
        return _tokens[Current];
    }

    /// <summary>
    /// Reads the next token from the token stream and advances forward.
    /// </summary>
    /// <returns>The next token</returns>
    public TokenType Read()
    {
        var tokens = _tokens[Current++];
		
        Line = tokens.Line;
		
        return tokens;
    }

    /// <summary>
    /// Returns the last token.
    /// </summary>
    /// <param name="previous">The number of tokens back to get from.</param>
    /// <returns>The last token</returns>
    public TokenType Last(int previous)
    {
        return _tokens[Current - previous];
    }

    /// <summary>
    /// Reads the next token, throwing an exception if there are no more tokens available or the resulting token is not the expected type.
    /// </summary>
    /// <param name="expectedId">The expected token type.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <returns>The next token.</returns>
    /// <exception cref="ParserException">Thrown if no more tokens are available or the token was the wrong type.</exception>
    public TokenType Consume(Id expectedId, [CallerMemberName] string memberName = "")
    {
        if (EndOfStream)
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
}