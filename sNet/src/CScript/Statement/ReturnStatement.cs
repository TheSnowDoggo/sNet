namespace sNet.CScriptPro;

public sealed class ReturnStatement : ExpressionStatement
{
	public ReturnStatement(int line) : base(line) { }

	public new static ReturnStatement Parse(CsrTokenStream stream)
	{
		int line = stream.Peek().Line;
		
		stream.Consume(CsrId.Return);

		var expr = ExpressionStatement.Parse(stream).Expression;

		return new ReturnStatement(line)
		{
			Expression = expr,
		};
	}

	public override ReturnValue Run(Context context)
	{
		var value = new Evaluator(context, Expression).Evaluate();
		
		return new ReturnValue(ReturnType.Return, value);
	}
}