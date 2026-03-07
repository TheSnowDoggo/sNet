using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class Event : Obj
{
	private readonly WeakSet<Function> _callbacks = [];

	public static readonly ReadOnlyTable Export = new Dictionary<Obj, Obj>()
	{
		{ "new", GlobalFunction.Create(New) },
	}.ToFrozenDictionary();
	
	public override TypeId TypeId => TypeId.Event;

	public override Obj this[Obj key] => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
	{
		"connect" => GlobalFunction.Create(Connect, TypeId.Function),
		"fire" => GlobalFunction.Create(Invoke, 0, -1),
		"clear" => GlobalFunction.Create(Clear),
		_ => Nil.Value,	
	};

	public void Invoke(Obj[] args)
	{
		foreach (var callback in _callbacks)
		{
			callback.Run(args);
		}
	}

	public void Invoke()
	{
		Invoke([]);
	}

	private static Event New(Obj[] args)
	{
		return new Event();
	}

	private Bool Connect(Obj[] args)
	{
		return _callbacks.Add((Function)args[0]);
	}

	private void Clear(Obj[] args)
	{
		_callbacks.Clear();
	}
}