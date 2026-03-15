namespace sNet.CScriptPro;

public abstract class Function : Obj
{
	public const int AnyArgs = -1;
	
	public override TypeId TypeId => TypeId.Function;
	
	public abstract string Name { get; }
	
	public int MinArgs { get; init; }
	public int MaxArgs { get; init; }
	public TypeId[] ArgTypes { get; init; }

	public Obj Run(params Obj[] args)
	{
		if (args.Length < MinArgs)
		{
			throw new InterpreterException($"{Name}() expected minimum of {MinArgs} arguments, got {args.Length}.");
		}

		if (MaxArgs != AnyArgs && args.Length > MaxArgs)
		{
			throw new InterpreterException($"{Name}() expected maximum of {MaxArgs} arguments, got {args.Length}.");
		}

		if (ArgTypes != null)
		{
			ValidateArguments(args);
		}

		return Invoke(args);
	}

	public bool TryRun(Obj[] args)
	{
		try
		{
			Run(args);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error($"{Name}() exception: {ex.Message}");
			return false;
		}
	}
	
	public override string ToString()
	{
		return $"{Name}()";
	}

	protected abstract Obj Invoke(Obj[] args);

	private void ValidateArguments(Obj[] args)
	{
		int count = Math.Min(args.Length, ArgTypes.Length);
		
		for (int i = 0; i < count; i++)
		{
			if (args[i].TypeId != ArgTypes[i])
			{
				throw new InterpreterException($"{Name}() expected argument {i + 1} to be type {ArgTypes[i]}, got {args[i].TypeId}.");
			}
		}
	}
}