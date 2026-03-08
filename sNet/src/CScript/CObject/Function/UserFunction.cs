namespace sNet.CScriptPro;

public sealed class UserFunction : Function
{
	public Context Parent { get; init; }
	public string[] Args { get; init; }
	public List<Statement> Statements { get; init; }
	
	public override string Name => RuntimeName;

	public string RuntimeName { get; set; }
	
	protected override Obj Invoke(Obj[] args)
	{
		var context = new Context(Parent);
		context.CreateScope();

		for (int i = 0; i < Args.Length; i++)
		{
			context.Define(Args[i], i < args.Length ? args[i] : Nil.Value);
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