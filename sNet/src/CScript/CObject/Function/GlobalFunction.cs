namespace sNet.CScriptPro;

public sealed class GlobalFunction : Function
{
	private readonly Func<CObj[], CObj> _func;

	public GlobalFunction(Func<CObj[], CObj> func)
	{
		_func = func;
	}

	public static GlobalFunction Create(Func<CObj[], CObj> func, int minArgs, int maxArgs, params TypeId[] types) => new GlobalFunction(func)
	{
		MinArgs = minArgs,
		MaxArgs = maxArgs,
		ArgTypes = types,
	};

	public static GlobalFunction Create(Func<CObj[], CObj> func, params TypeId[] types)
	{
		return Create(func, types.Length, types.Length, types);
	}

	public static GlobalFunction Create(Action<CObj[]> action, int minArgs, int maxArgs, params TypeId[] types)
	{
		return Create(Func, minArgs, maxArgs, types);

		CObj Func(CObj[] args)
		{
			action(args);
			return Nil.Value;
		}
	}
	
	public static GlobalFunction Create(Action<CObj[]> action, params TypeId[] types)
	{
		return Create(action, types.Length, types.Length, types);
	}

	protected override CObj Invoke(CObj[] args)
	{
		return _func.Invoke(args);
	}
}