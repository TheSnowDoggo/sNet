namespace sNet.CScriptPro;

public sealed class UserTable : ReadOnlyTable
{
	private readonly Dictionary<Obj, Obj> _values;

	public UserTable()
		: this([])
	{
	}
	
	public UserTable(Dictionary<Obj, Obj> values)
		: base(values)
	{
		_values = values;
	}

	public override Obj this[Obj key]
	{
		set
		{
			if (value == Nil.Value)
			{
				_values.Remove(key);
				return;
			}
			
			_values[key] = value;
		}
	}
}