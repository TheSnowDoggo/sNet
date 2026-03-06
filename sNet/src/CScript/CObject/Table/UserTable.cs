namespace sNet.CScriptPro;

public sealed class UserTable : ReadOnlyTable
{
	private readonly Dictionary<CObj, CObj> _values;

	public UserTable()
		: this([])
	{
	}
	
	public UserTable(Dictionary<CObj, CObj> values)
		: base(values)
	{
		_values = values;
	}

	public override CObj this[CObj key]
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