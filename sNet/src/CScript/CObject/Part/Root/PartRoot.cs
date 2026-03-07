namespace sNet.CScriptPro;

public abstract class PartRoot
{
	public PartRoot()
	{
		Root = new Part
		{
			Root = this,
		};
	}

	public readonly Part Root;

	public virtual void Update(double delta)
	{
		Event.Update.Fire([delta]);
	}

	public virtual void PartAdded(Part root)
	{
	}

	public virtual void PartRemoved(Part root)
	{
	}

	public virtual void PropertyUpdate(Part part, string name, Obj value)
	{
	}
}