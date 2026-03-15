namespace sNet.CScriptPro;

public sealed class WhileStatement : BlockStatement
{
	public WhileStatement(int line) : base(line) { }
	
	public List<CsrToken> Expression { get; init; }

	public new static WhileStatement Parse(CsrTokenStream stream)
	{
		int line = stream.Peek().Line;
		
		stream.Consume(CsrId.While);

		var expr = new RpnParser(stream).Parse(CsrId.OpenBrace);

		var statements = BlockStatement.Parse(stream).Statements;

		return new WhileStatement(line)
		{
			Expression = expr,
			Statements = statements,
		};
	}

	public override ReturnValue Run(Context context)
	{
		var evaluator = new Evaluator(context, Expression);
		
		while (evaluator.Evaluate().AsBool())
		{
			var returnValue = base.Run(context);

			if (ReturnValue.TryExit(ref returnValue))
			{
				return returnValue;
			}
		}

		return ReturnValue.None;
	}
}