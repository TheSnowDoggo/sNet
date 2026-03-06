namespace sNet.CScriptPro;

public sealed class UpdateEvent
{
	public UpdateEvent(string name, Obj value)
	{
		Name = name;
		Value = value;
	}
	
	public string Name { get; }
	public Obj Value { get; }
}