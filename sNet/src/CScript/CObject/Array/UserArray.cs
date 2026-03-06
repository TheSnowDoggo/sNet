namespace sNet.CScriptPro;

public sealed class UserArray : ArrayBase
{
	private readonly List<CObj> _items = [];

	public UserArray() { }

	public UserArray(IEnumerable<CObj> items)
		: this()
	{
		_items = new List<CObj>(items);
	}
	
	public UserArray(List<CObj> items)
		: this()
	{
		_items = items;
	}
	
	public override int Count => _items.Count;

	public override CObj this[CObj key] => key.TypeId switch
	{
		TypeId.String => GetMember((CStr)key),
		TypeId.Number => base[key],
		_ => Nil.Value,
	};

	public override CObj this[int index]
	{
		get => _items[index];
		set => _items[index] = value;
	}

	public override IEnumerator<CObj> GetEnumerator()
	{
		return _items.GetEnumerator();
	}

	private CObj GetMember(CStr member) => (string)member switch
	{
		"length" => (Number)_items.Count,
		"add" => GlobalFunction.Create(Add, 1, 1),
		"remove" => GlobalFunction.Create(Remove, 1, 1),
		"indexOf" => GlobalFunction.Create(IndexOf, TypeId.String),
		"contains" => GlobalFunction.Create(Contains, TypeId.String),
		"clear" => GlobalFunction.Create(Clear),
		_ => Nil.Value,
	};
	
	private void Add(CObj[] args)
	{
		_items.Add(args[0]);
	}

	private Bool Remove(CObj[] args)
	{
		return _items.Remove(args[0]);
	}

	private Number IndexOf(CObj[] args)
	{
		return _items.IndexOf(args[0]);
	}

	private Bool Contains(CObj[] args)
	{
		return _items.Contains(args[0]);
	}
	
	private void Clear(CObj[] args)
	{
		_items.Clear();
	}
}