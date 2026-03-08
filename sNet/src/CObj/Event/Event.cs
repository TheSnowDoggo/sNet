using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class Event : Obj
{
	private readonly HashSet<Function> _callbacks = [];
	
	public static readonly Event Update = new Event();

	public static readonly ReadOnlyTable Export = new Dictionary<Obj, Obj>()
	{
		{ "new", GlobalFunction.Create(New) },
		{ "update", Update },
	}.ToFrozenDictionary();
	
	public override TypeId TypeId => TypeId.Event;

	public override Obj this[Obj key] => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
	{
		"connect" => GlobalFunction.Create(Connect, TypeId.Function),
		"disconnect" => GlobalFunction.Create(Disconnect, TypeId.Function),
		"fire" => GlobalFunction.Create(Fire, 0, -1),
		"clear" => GlobalFunction.Create(Clear),
		_ => Nil.Value,	
	};

	public void Fire(params Obj[] args)
	{
		foreach (var callback in _callbacks)
		{
			callback.TryRun(args);
		}
	}

	public void Clear()
	{
		_callbacks.Clear();
	}

	private static Event New(Obj[] args)
	{
		return new Event();
	}

	private Bool Connect(Obj[] args)
	{
		return _callbacks.Add((Function)args[0]);
	}

	private Bool Disconnect(Obj[] args)
	{
		return _callbacks.Remove((Function)args[0]);
	}

	private void Clear(Obj[] args)
	{
		_callbacks.Clear();
	}
}