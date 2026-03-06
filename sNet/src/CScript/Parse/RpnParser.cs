using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class RpnParser
{
	private const int AssignPrecedence = 0;
	private const int UnaryPrecedence  = 11;
	
	private static readonly FrozenDictionary<CsrId, int> Precedence = new Dictionary<CsrId, int>()
	{
		{ CsrId.Invoke, 12 },
		{ CsrId.Period, 12 },
		{ CsrId.DynamicCast, 12 },
		{ CsrId.Complement, UnaryPrecedence },
		{ CsrId.Not, UnaryPrecedence },
		{ CsrId.UnaryMinus, UnaryPrecedence },
		{ CsrId.Cast, UnaryPrecedence },
		{ CsrId.Typeof, UnaryPrecedence },
		{ CsrId.Mul, 10 },
		{ CsrId.Div, 10 },
		{ CsrId.Rem, 10 },
		{ CsrId.Add, 9 },
		{ CsrId.Sub, 9 },
		{ CsrId.ShiftLeft, 8 },
		{ CsrId.ShiftRight, 8 },
		{ CsrId.LessThan, 7 },
		{ CsrId.GreaterThan, 7 },
		{ CsrId.LessThanOrEqual, 7 },
		{ CsrId.GreaterThanOrEqual, 7 },
		{ CsrId.Equals, 6 },
		{ CsrId.NotEquals, 6 },
		{ CsrId.And, 5 },
		{ CsrId.Xor, 2 },
		{ CsrId.Or, 3 },
		{ CsrId.Assign, AssignPrecedence },
		{ CsrId.OpenParen, int.MinValue },
	}.ToFrozenDictionary();

	private static readonly FrozenDictionary<CsrId, CsrId> UnaryMap = new Dictionary<CsrId, CsrId>()
	{
		{ CsrId.Sub, CsrId.UnaryMinus },
	}.ToFrozenDictionary();

	private readonly CsrTokenStream _stream;
	private List<CsrToken> _result;
	private Stack<CsrToken> _opStack;
	private int _level;

	public RpnParser(CsrTokenStream stream)
	{
		_stream = stream;
	}
	
	public List<CsrToken> Parse(params HashSet<CsrId> end)
	{
		_result = [];
		_opStack = [];
		_level = 0;

		while (!_stream.EndOfStream() && (_level > 0 || !end.Contains(_stream.Peek().Type)))
		{
			var token = _stream.Read();

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
				if ((_stream.Current == 0 || Precedence.ContainsKey(_stream.LastLast().Type) && !_stream.LastLast().IsCompound()) &&
				    UnaryMap.TryGetValue(token.Type, out var unaryType))
				{
					token.Type = unaryType;
				}

				LoadOperator(token);
				
				break;
			}
		}

		FlushAll();

		var result = _result;
		_result = null;

		_opStack = null;

		return result;
	}

	private static bool IsOpen(CsrToken csrToken)
	{
		return csrToken.Type is CsrId.OpenParen or CsrId.OpenSquare;
	}

	private void ParseImplicit(int line)
	{
		if (_stream.EndOfStream())
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
		
		csrToken.Value = new CType(typeId);
		
		LoadOperator(csrToken);
	}

	private int GetPrecedence(CsrToken csrToken)
	{
		if (csrToken.IsCompound())
		{
			return AssignPrecedence;
		}
		
		if (Precedence.TryGetValue(csrToken.Type, out var precedence))
		{
			return precedence;
		}

		throw new ParserException(_stream.Line, $"Unrecognized operator {csrToken.Type}.");
	}

	private bool ShouldFlush(CsrToken peek, CsrToken csrToken, int precedence)
	{
		int opPrecedence = GetPrecedence(peek);

		if (opPrecedence > precedence)
		{
			return true;
		}

		if (opPrecedence < precedence)
		{
			return false;
		}

		if (opPrecedence == AssignPrecedence && precedence == AssignPrecedence)
		{
			return false;
		}

		if (opPrecedence == UnaryPrecedence && precedence == UnaryPrecedence)
		{
			return false;
		}

		return true;
	}
	
	private void LoadOperator(CsrToken csrToken)
	{
		int precedence = GetPrecedence(csrToken);

		while (_opStack.TryPeek(out var op) && ShouldFlush(op, csrToken, precedence))
		{
			TransferOperator();
		}
		
		_opStack.Push(csrToken);
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

		if (stream.EndOfStream())
		{
			throw new ParserException(stream.Line, "Expected close bracket at end of argument list, got nothing.");
		}
		
		if (stream.Peek().Type == CsrId.CloseParen)
		{
			return 0;
		}

		int level = 0;
		int args = 1;

		while (!stream.EndOfStream())
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