namespace sNet.CScriptPro;

public sealed class TableDefinition : Obj
{
	public sealed record Pair(List<CsrToken> KeyExpression, List<CsrToken> ValueExpression);

	private readonly List<Pair> _items;

	private TableDefinition(List<Pair> items)
	{
		_items = items;
	}

	public override TypeId TypeId => TypeId.Nil;

	public static TableDefinition Parse(CsrTokenStream stream)
	{
		var items = new List<Pair>();
		
		while (!stream.EndOfStream && stream.Peek().Type != CsrId.CloseBrace)
		{
			List<CsrToken> keyExpr;
			List<CsrToken> valExpr;

			var head = stream.Read();

			switch (head.Type)
			{
			case CsrId.OpenSquare:
				keyExpr = new RpnParser(stream).Parse(CsrId.CloseSquare);
				
				stream.Consume(CsrId.CloseSquare);
				stream.Consume(CsrId.Assign);
				
				valExpr = new RpnParser(stream).Parse(CsrId.Comma, CsrId.CloseBrace);
				break;
			case CsrId.Identifier:
				head.Type = CsrId.Literal;
				head.Value = (StrObj)head.Lexeme;
				
				keyExpr = [head];
				
				stream.Consume(CsrId.Assign);
				
				valExpr = new RpnParser(stream).Parse(CsrId.Comma, CsrId.CloseBrace);
				break;
			case CsrId.Function:
				var name = stream.Consume(CsrId.Identifier);
				name.Type = CsrId.Literal;
				name.Value = (StrObj)name.Lexeme;

				keyExpr = [name];
				valExpr = [FunctionDefinition.ParseLambda(stream).ToToken(stream.Line)];
				
				break;
			default:
				throw new ParserException(stream.Line, $"Unexpected token {stream.Peek().Type}, expected function, identifier or [expression].");
			}

			items.Add(new Pair(keyExpr, valExpr));

			if (stream.EndOfStream)
			{
				throw new ParserException(stream.Line, "Expected comma or end brace, got nothing.");
			}

			if (stream.Peek().Type == CsrId.CloseBrace)
			{
				break;
			}

			stream.Consume(CsrId.Comma);
		}

		stream.Consume(CsrId.CloseBrace);

		return new TableDefinition(items);
	}

	public UserTable Create(Context context)
	{
		var table = new UserTable();

		foreach (var item in _items)
		{
			var key = new Evaluator(context, item.KeyExpression).Evaluate();
			var value = new Evaluator(context, item.ValueExpression).Evaluate();

			table[key] = value;
		}

		return table;
	}
	
	public CsrToken ToToken(int line)
	{
		return new CsrToken(line, CsrId.TableDefinition, "table_def", this);
	}
}