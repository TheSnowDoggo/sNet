namespace sNet.CScriptPro;

public sealed class ArrayObj : ArrayBaseObj
{
	private readonly List<Obj> _items = [];

	public ArrayObj() { }

	public ArrayObj(IEnumerable<Obj> items)
		: this()
	{
		_items = new List<Obj>(items);
	}
	
	public ArrayObj(List<Obj> items)
		: this()
	{
		_items = items;
	}
	
	public override int Count => _items.Count;

	public override Obj this[Obj key] => key.TypeId switch
	{
		TypeId.String => GetMember((StrObj)key),
		TypeId.Number => base[key],
		_ => Nil.Value,
	};

	public override Obj this[int index]
	{
		get => _items[index];
		set => _items[index] = value;
	}

	public override IEnumerator<Obj> GetEnumerator()
	{
		return _items.GetEnumerator();
	}

	private Obj GetMember(StrObj member) => (string)member switch
	{
		"length" => (Number)_items.Count,
		"add" => GlobalFunction.Create(Add, 1, 1),
		"remove" => GlobalFunction.Create(Remove, 1, 1),
		"indexOf" => GlobalFunction.Create(IndexOf, TypeId.String),
		"contains" => GlobalFunction.Create(Contains, TypeId.String),
		"clear" => GlobalFunction.Create(Clear),
		_ => Nil.Value,
	};
	
	private void Add(Obj[] args)
	{
		_items.Add(args[0]);
	}

	private Bool Remove(Obj[] args)
	{
		return _items.Remove(args[0]);
	}

	private Number IndexOf(Obj[] args)
	{
		return _items.IndexOf(args[0]);
	}

	private Bool Contains(Obj[] args)
	{
		return _items.Contains(args[0]);
	}
	
	private void Clear(Obj[] args)
	{
		_items.Clear();
	}
}