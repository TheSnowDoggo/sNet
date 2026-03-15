namespace sNet.CScriptPro;

public sealed class ForeachStatement : BlockStatement
{
	public ForeachStatement(int line) : base(line) { }

	public string Name { get; init; }
	public string AltName { get; init; }
	public List<CsrToken> Expression { get; init; }
	
	public new static ForeachStatement Parse(CsrTokenStream stream)
	{
		int line = stream.Peek().Line;

		stream.Consume(CsrId.Foreach);

		string name = stream.Consume(CsrId.Identifier).Lexeme;

		string altName = null;
		
		if (stream.Peek().Type == CsrId.Comma)
		{
			stream.Read();

			altName = stream.Consume(CsrId.Identifier).Lexeme;
		}

		stream.Consume(CsrId.In);

		var expr = new RpnParser(stream).Parse(CsrId.OpenBrace);

		var statements = BlockStatement.Parse(stream).Statements;

		return new ForeachStatement(line)
		{
			Name = name,
			AltName = altName,
			Expression = expr,
			Statements = statements,
		};
	}

	public override ReturnValue Run(Context context)
	{
		context.CreateScope();
		
		var obj = new Evaluator(context, Expression).Evaluate();

		if (obj.TypeId == TypeId.Table)
		{
			return EnumerateTable(context, (ReadOnlyTable)obj);
		}
		
		return EnumerateFlat(context, obj);
	}
	
	private ReturnValue EnumerateTable(Context context, ReadOnlyTable table)
	{
		if (AltName == null)
		{
			throw new InterpreterException(context.Line, "Enumerating table expected [key, value], only got [key].");
		}
		
		context.Define(Name, Nil.Value);
		context.Define(AltName, Nil.Value);
		
		foreach (var kvp in table)
		{
			context[Name] = kvp.Key;
			context[AltName] = kvp.Value;
			
			var returnValue = base.Run(context);

			if (ReturnValue.TryExit(ref returnValue, context))
			{
				return returnValue;
			}
		}
		
		return ReturnValue.None;
	}

	private ReturnValue EnumerateFlat(Context context, Obj obj)
	{
		if (obj is not IEnumerable<Obj> enumerable)
		{
			throw new InterpreterException(context.Line, $"Type {obj.TypeId} is not enumerable.");
		}
		
		if (AltName != null)
		{
			throw new InterpreterException(context.Line, "Enumerating collection expected [key], but got [key, value].");
		}
		
		context.Define(Name, Nil.Value);

		foreach (Obj item in enumerable)
		{
			context[Name] = item;
			
			var returnValue = base.Run(context);

			if (ReturnValue.TryExit(ref returnValue, context))
			{
				return returnValue;
			}
		}
		
		return ReturnValue.None;
	}
}