using sNet.CScriptPro;

namespace sNet;

public sealed class PartLoader
{
	private readonly Dictionary<string, PartTag> _parts = [];
	
	public static PartLoader Default { get; set; } = new PartLoader();

	public bool TryGet(string source, out PartTag tag)
	{
		if (_parts.TryGetValue(source, out tag))
		{
			return true;
		}

		try
		{
			_parts[source] = tag = PartTag.Parse(source);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
	}
}