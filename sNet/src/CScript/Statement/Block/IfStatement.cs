namespace sNet.CScriptPro;

public sealed class IfStatement : BlockStatement
{
	public IfStatement(int line) : base(line) { }

	public List<CsrToken> Expression { get; init; }
	public BlockStatement Else { get; init; }
	
	public new static IfStatement Parse(CsrTokenStream stream)
	{
		int line = stream.Peek().Line;
		
		stream.Consume(CsrId.If);

		var expr = new RpnParser(stream).Parse(CsrId.OpenBrace);

		var statements = BlockStatement.Parse(stream).Statements;

		if (stream.EndOfStream || stream.Peek().Type != CsrId.Else)
		{
			return new IfStatement(line)
			{
				Expression = expr,
				Statements = statements,
			};
		}

		stream.Read();

		if (stream.EndOfStream)
		{
			throw new ParserException(stream.Line, "Expected block or if proceeding else keyword, got nothing.");
		}

		BlockStatement elseBlock = stream.Peek().Type switch
		{
			CsrId.If => Parse(stream),
			CsrId.OpenBrace => BlockStatement.Parse(stream),
			_ => throw new ParserException(stream.Line, $"Expected block or if proceeding else keyword, got {stream.Peek().Type}."),
		};

		return new IfStatement(line)
		{
			Expression = expr,
			Statements = statements,
			Else = elseBlock,
		};
	}

	public override ReturnValue Run(Context context)
	{
		var result = new Evaluator(context, Expression).Evaluate();

		if (result.AsBool())
		{
			return base.Run(context);
		}

		if (Else != null)
		{
			return Else.Run(context);
		}

		return ReturnValue.None;
	}
}