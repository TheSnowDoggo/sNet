namespace sNet.CScriptPro;

public sealed class ValueStatement : Statement
{
	public ValueStatement(int line) : base(line) { }

	public ReturnType ReturnType { get; init; }

	public static ValueStatement Parse(CsrTokenStream stream, ReturnType returnType)
	{
		int line = stream.Read().Line;
		stream.Consume(CsrId.Semicolon);

		return new ValueStatement(line)
		{
			ReturnType = returnType,
		};
	}

	public override ReturnValue Run(Context context)
	{
		return new ReturnValue(ReturnType);
	}
}