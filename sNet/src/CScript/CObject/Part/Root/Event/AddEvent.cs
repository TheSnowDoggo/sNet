namespace sNet.CScriptPro;

public sealed class AddEvent
{
	public AddEvent(Uid parent, Part part)
	{
		Parent = parent;
		Part = part;
	}
	
	public Uid Parent { get; }
	public Part Part { get; }

	public void Deconstruct(out Uid parent, out Part part)
	{
		parent = Parent;
		part = Part;
	}
}