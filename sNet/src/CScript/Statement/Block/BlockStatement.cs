namespace sNet.CScriptPro;

public class BlockStatement : Statement
{
	public BlockStatement(int line) : base(line) { }

	public List<Statement> Statements { get; init; }

	public new static BlockStatement Parse(CsrTokenStream stream)
	{
		int line = stream.Peek().Line;

		stream.Consume(CsrId.OpenBrace);
		
		var statements = new List<Statement>();
		
		while (!stream.EndOfStream && stream.Peek().Type != CsrId.CloseBrace)
		{
			statements.Add(Statement.Parse(stream));
		}
		
		stream.Consume(CsrId.CloseBrace);

		return new BlockStatement(line)
		{
			Statements = statements,
		};
	}
	
	public override ReturnValue Run(Context context)
	{
		base.Run(context);

		context.CreateScope();
		
		var returnValue = ReturnValue.None;
		
		foreach (var statement in Statements)
		{
			returnValue = statement.Run(context);

			if (returnValue.Type != ReturnType.None)
			{
				break;
			}
		}

		context.CloseScope();
		
		return returnValue;
	}
}