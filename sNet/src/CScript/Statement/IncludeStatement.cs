namespace sNet.CScriptPro;

public sealed class IncludeStatement : Statement
{
	public IncludeStatement(int line) : base(line) { }

	public string Name { get; init; }
	
	public new static IncludeStatement Parse(CsrTokenStream stream)
	{
		int line = stream.Peek().Line;
		
		stream.Consume(CsrId.Include);

		var name = stream.Consume(CsrId.Identifier).Lexeme;

		stream.Consume(CsrId.Semicolon);

		return new IncludeStatement(line)
		{
			Name = name,
		};
	}

	public override ReturnValue Run(Context context)
	{
		base.Run(context);

		var packetManager = context.GetPacketManager();
		
		if (packetManager == null)
		{
			throw new InterpreterException(context.Line, "No package manager is loaded.");
		}
		
		if (!packetManager.Packages.TryGetValue(Name, out var package))
		{
			throw new InterpreterException(context.Line, $"Package ('{Name}') not found.");
		}

		foreach ((Obj name, Obj value) in package.Export)
		{
			if (name.TypeId != TypeId.String)
			{
				continue;
			}

			try
			{
				context.Define((string)name, value);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message);
			}
		}
		
		return ReturnValue.None;
	}
}