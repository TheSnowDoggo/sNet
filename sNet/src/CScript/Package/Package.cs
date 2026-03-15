namespace sNet.CScriptPro;

public abstract class Package
{
	public abstract string Name { get; }
	public abstract ReadOnlyTable Export { get; }
}