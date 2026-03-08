namespace sNet.CScriptPro;

public sealed class GSProperty<TPart, TValue> : IProperty
	where TPart : Part
	where TValue : Obj
{
	private readonly Func<TPart, TValue> _getter;
	private readonly Action<TPart, TValue> _setter;
	private readonly TypeId _type;

	public GSProperty(Func<TPart, TValue> getter, Action<TPart, TValue> setter, TypeId type, bool serializable = true)
	{
		_getter = getter;
		_setter = setter;
		_type = type;
		Serializable = serializable;
	}

	public bool Serializable { get; }

	public Obj this[Part part]
	{
		get => _getter((TPart)part);
		set => _setter((TPart)part, (TValue)value.Expect(_type));
	}

	public int Serialize(Part part, Stream stream)
	{
		return stream.WriteObj(this[part]);
	}

	public void Deserialize(Part part, Stream stream)
	{
		this[part] = stream.ReadObj();
	}
}