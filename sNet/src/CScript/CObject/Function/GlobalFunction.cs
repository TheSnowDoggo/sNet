namespace sNet.CScriptPro;

public sealed class GlobalFunction : Function
{
	private readonly Func<Obj[], Obj> _func;

	public GlobalFunction(Func<Obj[], Obj> func)
	{
		_func = func;
	}

	public override string Name => _func.Method.Name;

	public static GlobalFunction Create(Func<Obj[], Obj> func, int minArgs, int maxArgs, params TypeId[] types) => new GlobalFunction(func)
	{
		MinArgs = minArgs,
		MaxArgs = maxArgs,
		ArgTypes = types,
	};

	public static GlobalFunction Create(Func<Obj[], Obj> func, params TypeId[] types)
	{
		return Create(func, types.Length, types.Length, types);
	}

	public static GlobalFunction Create(Action<Obj[]> action, int minArgs, int maxArgs, params TypeId[] types)
	{
		return Create(Func, minArgs, maxArgs, types);

		Obj Func(Obj[] args)
		{
			action(args);
			return Nil.Value;
		}
	}
	
	public static GlobalFunction Create(Action<Obj[]> action, params TypeId[] types)
	{
		return Create(action, types.Length, types.Length, types);
	}

	protected override Obj Invoke(Obj[] args)
	{
		return _func.Invoke(args);
	}
}