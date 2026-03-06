namespace sNet.CScriptPro;

public sealed class ArrayDef : Obj
{
	public ArrayDef() : base(TypeId.Nil) { }
	
	public List<List<CsrToken>> Expressions { get; init; }

	public static ArrayDef Parse(CsrTokenStream stream)
	{
		var expressions = new List<List<CsrToken>>();

		var parser = new RpnParser(stream);

		while (!stream.EndOfStream() && stream.Peek().Type != CsrId.CloseSquare)
		{
			expressions.Add(parser.Parse(CsrId.Comma, CsrId.CloseSquare));

			if (stream.EndOfStream())
			{
				throw new ParserException(stream.Line, "Expected comma or close bracket, ran out of tokens.");
			}

			if (stream.Peek().Type == CsrId.CloseSquare)
			{
				break;
			}

			stream.Consume(CsrId.Comma);
		}

		stream.Consume(CsrId.CloseSquare);

		return new ArrayDef()
		{
			Expressions = expressions,
		};
	}

	public ArrayObj Create(Context context)
	{
		var list = new List<Obj>(Expressions.Count);

		foreach (var expression in Expressions)
		{
			list.Add(new Evaluator(context, expression).Evaluate());
		}
		
		return new ArrayObj(list);
	}
	
	public CsrToken ToToken(int line)
	{
		return new CsrToken(line, CsrId.ArrayDefinition, "array_def", this);
	}
}