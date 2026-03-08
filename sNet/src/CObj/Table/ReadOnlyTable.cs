using System.Collections;
using System.Collections.Frozen;
using System.Text;

namespace sNet.CScriptPro;

public class ReadOnlyTable : Obj,
	IReadOnlyDictionary<Obj, Obj>
{
	private readonly IReadOnlyDictionary<Obj, Obj> _dictionary;
	
	public ReadOnlyTable(IReadOnlyDictionary<Obj, Obj> dictionary)
	{
		_dictionary = dictionary;
	}
	
	public int Count => _dictionary.Count;

	public override TypeId TypeId => TypeId.Table;

	public IEnumerable<Obj> Keys => _dictionary.Keys;
	public IEnumerable<Obj> Values => _dictionary.Values;

	public override Obj this[Obj key] => _dictionary.GetValueOrDefault(key, Nil.Value);
	
	public static implicit operator ReadOnlyTable(FrozenDictionary<Obj, Obj> dictionary)
		=> new ReadOnlyTable(dictionary);

	public bool ContainsKey(Obj key)
	{
		return _dictionary.ContainsKey(key);
	}

	public bool TryGetValue(Obj key, out Obj value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	public IEnumerator<KeyValuePair<Obj, Obj>> GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void Format(StringBuilder sb, int level)
	{
		sb.AppendLine("{");

		foreach (var kvp in _dictionary)
		{
			sb.Append(' ', (level + 1) * 2);
			sb.Append($"{kvp.Key}: ");

			if (kvp.Value is ReadOnlyTable table)
			{
				table.Format(sb, level + 1);
			}
			else
			{
				sb.Append(kvp.Value);
			}

			sb.AppendLine(",");
		}

		sb.Append(' ', level * 2);
		sb.Append('}');
	}
	
	public override string ToString()
	{
		var sb = new StringBuilder();
		Format(sb, 0);
		return sb.ToString();
	}
}