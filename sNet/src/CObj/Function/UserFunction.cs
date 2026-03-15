namespace sNet.CScriptPro;

public sealed class UserFunction : Function
{
	public Context Parent { get; init; }
	public Arg[] Args { get; init; }
	public List<Statement> Statements { get; init; }
	
	public override string Name => RuntimeName;

	public string RuntimeName { get; init; }
	
	protected override Obj Invoke(Obj[] args)
	{
		var context = new Context(Parent);
		context.CreateScope();

		for (int i = 0; i < Args.Length; i++)
		{
			context.Define(Args[i].Name, i < args.Length ? args[i] : Args[i].DefaultValue);
		}

		foreach (var statement in Statements)
		{
			var returnValue = statement.Run(context);

			switch (returnValue.Type)
			{
			case ReturnType.Return:
				return returnValue.Value;
			case ReturnType.Break:
				throw new InterpreterException(context.Line, "Cannot break out of function.");
			case ReturnType.Continue:
				throw new InterpreterException(context.Line, "Cannot continue in function.");
			}
		}
		
		return Nil.Value;
	}
}