using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class RpnParser
{
	private readonly CsrTokenStream _stream;
	private List<CsrToken> _result;
	private Stack<CsrToken> _opStack;
	private int _level;

	public RpnParser(CsrTokenStream stream)
	{
		_stream = stream;
	}
	
	public List<CsrToken> Parse(bool ignoreFirst, HashSet<CsrId> end)
	{
		_result = [];
		_opStack = [];
		_level = 0;

		var first = true;

		while (!_stream.EndOfStream && (ignoreFirst && first || _level > 0 || !end.Contains(_stream.Peek().Type)))
		{
			NextToken(_stream.Read());

			first = false;
		}

		FlushAll();

		var result = _result;
		_result = null;

		_opStack = null;

		return result;
	}
	
	public List<CsrToken> Parse(HashSet<CsrId> end)
	{
		return Parse(false, end);
	}

	private static bool IsOpen(CsrToken csrToken)
	{
		return csrToken.Type is CsrId.OpenParen or CsrId.OpenSquare;
	}

	private static bool IsOperator(CsrToken csrToken)
	{
		return CsrConfig.Precedence.ContainsKey(csrToken.Type);
	}

	private void NextToken(CsrToken token)
	{
		switch (token.Type)
		{
		case CsrId.OpenParen:
			_opStack.Push(token);
			_level++;
			break;
		case CsrId.OpenSquare:
			ParseArrayDefinition(token);
			break;
		case CsrId.OpenBrace:
			ParseTableDefinition(token);
			break;
		case CsrId.CloseParen:
			FlushBracket(CsrId.OpenParen);
			ParseImplicit(token.Line);
			_level--;
			break;
		case CsrId.CloseSquare:
			FlushBracket(CsrId.OpenSquare);
			ParseImplicit(token.Line);
			_level--;
			break;
		case CsrId.Comma:
			FlushComma();
			break;
		case CsrId.Literal:
			_result.Add(token);
			break;
		case CsrId.Identifier:
			_result.Add(token);
			ParseImplicit(token.Line);
			break;
		case CsrId.Function:
			ParseFunctionDefinition(token);
			break;
		case CsrId.Cast:
			ParseCast(token);
			break;
		default:
			LoadOperator(token);
			break;
		}
	}

	private void ParseImplicit(int line)
	{
		if (_stream.EndOfStream)
		{
			return;
		}

		switch (_stream.Peek().Type)
		{
		case CsrId.OpenParen:
			ParseInvoke(line);
			break;
		case CsrId.OpenSquare:
			ParseSquare(line);
			break;
		}
	}

	private void ParseInvoke(int line)
	{
		int args = GetArgCount(_stream.SubStream());
		
		LoadOperator(new CsrToken(line, CsrId.Invoke, $"invoke<{args}>", (Number)args));
	}

	private void ParseSquare(int line)
	{
		LoadOperator(new CsrToken(line, CsrId.Period, "access", Bool.True));

		_opStack.Push(_stream.Consume(CsrId.OpenSquare));
		_level++;
	}

	private void ParseFunctionDefinition(CsrToken csrToken)
	{
		_result.Add(FunctionDefinition.ParseLambda(_stream).ToToken(csrToken.Line));
	}

	private void ParseArrayDefinition(CsrToken csrToken)
	{
		_result.Add(ArrayDef.Parse(_stream).ToToken(csrToken.Line));
	}

	private void ParseTableDefinition(CsrToken csrToken)
	{
		_result.Add(TableDefinition.Parse(_stream).ToToken(csrToken.Line));
	}

	private void ParseCast(CsrToken csrToken)
	{
		_stream.Consume(CsrId.LessThan);
		
		var typeStr = _stream.Consume(CsrId.Identifier).Lexeme;

		if (!Enum.TryParse<TypeId>(typeStr, true, out var typeId))
		{
			throw new ParserException(_stream.Line, $"Unrecognised cast type (\'{typeStr}\').");
		}

		_stream.Consume(CsrId.GreaterThan);
		
		csrToken.Value = new TypeObj(typeId);
		
		LoadOperator(csrToken);
	}

	private int GetPrecedence(CsrToken csrToken)
	{
		if (csrToken.IsCompound())
		{
			return CsrConfig.AssignPrecedence;
		}
		
		if (CsrConfig.Precedence.TryGetValue(csrToken.Type, out int precedence))
		{
			return precedence;
		}

		throw new ParserException(_stream.Line, $"Unrecognized operator {csrToken.Type}.");
	}

	private bool ShouldFlush(CsrToken token, int precedence, CsrToken other)
	{
		if (IsOpen(other))
		{
			return false;
		}

		int otherPrecedence = GetPrecedence(other);

		if (precedence < otherPrecedence)
		{
			return true;
		}

		if (precedence > otherPrecedence)
		{
			return false;
		}

		if (CsrConfig.RightAssociative.Contains(token.Type) && CsrConfig.RightAssociative.Contains(other.Type))
		{
			return false;
		}

		return true;
	}
	
	private void LoadOperator(CsrToken token)
	{
		if ((_stream.Current == 0 || IsOperator(_stream.Last(2)) && !_stream.Last(2).IsCompound()) &&
		    CsrConfig.UnaryMap.TryGetValue(token.Type, out var unaryType))
		{
			token.Type = unaryType;
		}
		
		int precedence = GetPrecedence(token);
		
		while (_opStack.TryPeek(out var op) && ShouldFlush(token, precedence, op))
		{
			TransferOperator();
		}
		
		_opStack.Push(token);
	}
	
	private void FlushComma()
	{
		while (_opStack.TryPeek(out var token) && !IsOpen(token))
		{
			TransferOperator();
		}
	}

	private void FlushBracket(CsrId open)
	{
		while (_opStack.TryPeek(out var token) && token.Type != open)
		{
			TransferOperator();
		}

		if (_opStack.Count == 0)
		{
			throw new ParserException(_stream.Line, "Close bracket without matching open bracket.");
		}
		
		_opStack.Pop();
	}

	private void FlushAll()
	{
		while (_opStack.TryPeek(out var token))
		{
			if (IsOpen(token))
			{
				throw new ParserException(_stream.Line, $"Open bracket {token.Type} without matching close bracket.");
			}
			
			TransferOperator();
		}
	}

	private void TransferOperator()
	{
		_result.Add(_opStack.Pop());
	}

	private static int GetArgCount(CsrTokenStream stream)
	{
		stream.Consume(CsrId.OpenParen);

		if (stream.EndOfStream)
		{
			throw new ParserException(stream.Line, "Expected close bracket at end of argument list, got nothing.");
		}
		
		if (stream.Peek().Type == CsrId.CloseParen)
		{
			return 0;
		}

		int level = 0;
		int args = 1;

		while (!stream.EndOfStream)
		{
			var token = stream.Read();

			switch (token.Type)
			{
			case CsrId.OpenParen:
				level++;
				break;
			case CsrId.CloseParen:
				if (level == 0)
				{
					return args;
				}
				level--;
				break;
			case CsrId.Comma:
				if (level == 0)
				{
					args++;
				}
				break;
			}
		}

		throw new ParserException(stream.Line, "Expected close bracket at end of argument list, got nothing.");
	}
}