namespace sNet.CScriptPro;

public sealed class ImportStatement : Statement
{
	public ImportStatement(int line) : base(line) { }
	
	public string Name { get; init; }
	public string Alias { get; init; }

	public new static ImportStatement Parse(CsrTokenStream stream)
	{
		int line = stream.Peek().Line;
		
		stream.Consume(CsrId.Import);

		string name = stream.Consume(CsrId.Identifier).Lexeme;

		if (stream.EndOfStream())
		{
			throw new ParserException(stream.Line, "Expected as or semicolon, ran out of tokens.");
		}

		string alias = null;

		if (stream.Peek().Type == CsrId.As)
		{
			stream.Read();
			alias = stream.Consume(CsrId.Identifier).Lexeme;
		}

		stream.Consume(CsrId.Semicolon);

		return new ImportStatement(line)
		{
			Name = name,
			Alias = alias,
		};
	}

	public override ReturnValue Run(Context context)
	{
		base.Run(context);

		if (PackageManager.Current == null)
		{
			throw new InterpreterException(context.Line, "No package manager is loaded.");
		}
		
		if (!PackageManager.Current.Packages.TryGetValue(Name, out var package))
		{
			throw new InterpreterException(context.Line, $"Package ('{Name}') not found.");
		}

		context.Define(Alias ?? Name, package.Export, VariableAttribute.Const);

		return ReturnValue.None;
	}
}