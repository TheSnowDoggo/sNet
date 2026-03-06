namespace sNet.CScriptPro;

public abstract class Function : CObj
{
	public const int AnyArgs = -1;
	
	public Function() : base(TypeId.Function) { }
	
	public int MinArgs { get; init; }
	public int MaxArgs { get; init; }
	public TypeId[] ArgTypes { get; init; }

	public CObj Run(CObj[] args)
	{
		if (args.Length < MinArgs)
		{
			throw new InterpreterException($"Function expected minimum of {MinArgs} arguments, got {args.Length}.");
		}

		if (MaxArgs != AnyArgs && args.Length > MaxArgs)
		{
			throw new InterpreterException($"Function expected maximum of {MaxArgs} arguments, got {args.Length}.");
		}

		if (ArgTypes != null)
		{
			ValidateArguments(args);
		}

		return Invoke(args);
	}
	
	public CObj Run()
	{
		return Run([]);
	}

	protected abstract CObj Invoke(CObj[] args);

	private void ValidateArguments(CObj[] args)
	{
		int count = Math.Min(args.Length, ArgTypes.Length);
		
		for (int i = 0; i < count; i++)
		{
			if (args[i].TypeId != ArgTypes[i])
			{
				throw new InterpreterException($"Function expected argument {i + 1} to be type {ArgTypes[i]}, got {args[i].TypeId}.");
			}
		}
	}
}