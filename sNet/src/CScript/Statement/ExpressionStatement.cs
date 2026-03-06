namespace sNet.CScriptPro;

public class ExpressionStatement : Statement
{
	public ExpressionStatement(int line) : base(line) { }

	public List<CsrToken> Expression { get; init; }

	public new static ExpressionStatement Parse(CsrTokenStream stream)
	{
		int line = stream.Peek().Line;
		
		var expr = new RpnParser(stream).Parse(CsrId.Semicolon);
		
		stream.Consume(CsrId.Semicolon);

		return new ExpressionStatement(line)
		{
			Expression = expr,
		};
	}
	
	public override ReturnValue Run(Context context)
	{
		base.Run(context);

		new Evaluator(context, Expression).Evaluate();
		
		return ReturnValue.None;
	}
}