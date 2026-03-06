using System.Collections;
using System.Collections.Frozen;
using System.Text;

namespace sNet.CScriptPro;

public class ReadOnlyTable : CObj,
	IReadOnlyDictionary<CObj, CObj>
{
	private readonly IReadOnlyDictionary<CObj, CObj> _dictionary;
	
	public ReadOnlyTable(IReadOnlyDictionary<CObj, CObj> dictionary)
		: base(TypeId.Table)
	{
		_dictionary = dictionary;
	}
	
	public int Count => _dictionary.Count;

	public IEnumerable<CObj> Keys => _dictionary.Keys;
	public IEnumerable<CObj> Values => _dictionary.Values;

	public override CObj this[CObj key] => _dictionary.GetValueOrDefault(key, Nil.Value);
	
	public static implicit operator ReadOnlyTable(FrozenDictionary<CObj, CObj> dictionary)
		=> new ReadOnlyTable(dictionary);

	public bool ContainsKey(CObj key)
	{
		return _dictionary.ContainsKey(key);
	}

	public bool TryGetValue(CObj key, out CObj value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	public IEnumerator<KeyValuePair<CObj, CObj>> GetEnumerator()
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