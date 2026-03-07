namespace sNet.CScriptPro;

public sealed class ForStatement : BlockStatement
{
	public ForStatement(int line) : base(line) { }
	
	public DefineStatement Define { get; init; }
	public List<CsrToken> Condition { get; init; }
	public List<CsrToken> Increment{ get; init; }
	
	protected override bool CreateScope => false;

	public new static ForStatement Parse(CsrTokenStream stream)
	{
		int line = stream.Peek().Line;
		
		stream.Consume(CsrId.For);

		var define = DefineStatement.Parse(stream);

		var condition = new RpnParser(stream).Parse(CsrId.Semicolon);

		stream.Consume(CsrId.Semicolon);

		var increment = new RpnParser(stream).Parse(CsrId.OpenBrace);

		var statements = BlockStatement.Parse(stream).Statements;

		return new ForStatement(line)
		{
			Define = define,
			Condition = condition,
			Increment = increment,
			Statements = statements,
		};
	}

	public override ReturnValue Run(Context context)
	{
		context.CreateScope();
		
		Define.Run(context);
		
		var condition = new Evaluator(context, Condition);
		var increment = new Evaluator(context, Increment);
		
		var returnValue = ReturnValue.None;

		while (condition.Evaluate().AsBool())
		{
			returnValue = base.Run(context);

			if (returnValue.Type == ReturnType.Return)
			{
				break;
			}

			if (returnValue.Type == ReturnType.Break)
			{
				returnValue = ReturnValue.None;
				break;
			}
			
			increment.Evaluate();
		}
		
		context.CloseScope();
		
		return returnValue;
	}
}