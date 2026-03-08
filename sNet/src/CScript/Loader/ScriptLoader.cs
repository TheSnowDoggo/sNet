using sNet.CScriptPro;

namespace sNet;

public sealed class ScriptLoader
{
	private readonly Dictionary<string, FunctionDefinition> _scripts = [];
	
	public static ScriptLoader Default { get; set; } = new ScriptLoader();

	public bool TryGet(string source, out FunctionDefinition definition)
	{
		if (_scripts.TryGetValue(source, out definition))
		{
			return true;
		}

		try
		{
			_scripts[source] = definition = FunctionDefinition.ParseMain(source);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to load script at {source}: {ex.Message}");
			return false;
		}
	}
}