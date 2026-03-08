namespace sNet.CScriptPro;

public sealed class Arg
{
	public Arg(string name, Obj defaultValue)
	{
		Name = name;
		DefaultValue = defaultValue;
	}
	
	public string Name { get; }
	public Obj DefaultValue { get; }
}