namespace sNet.CScriptPro;

[Flags]
public enum PartFlag
{
	None = 0,
	NoAuto = 1 << 0,
	Root = 1 << 1,
}