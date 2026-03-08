namespace sNet.CScriptPro;

public sealed class GProperty<TPart> : IProperty
	where TPart : Part
{
	private readonly Func<TPart, Obj> _getter;

	public GProperty(Func<TPart, Obj> getter)
	{
		_getter = getter;
	}

	public bool Serializable => false;

	public static implicit operator GProperty<TPart>(Func<TPart, Obj> getter)
		=> new GProperty<TPart>(getter);
	
	public Obj this[Part part]
	{
		get => _getter((TPart)part);
		set { }
	}

	public int Serialize(Part part, Stream stream)
	{
		throw new InvalidOperationException("Property is not serializable.");
	}

	public void Deserialize(Part part, Stream stream)
	{
		throw new InvalidOperationException("Property is not serializable.");
	}
}