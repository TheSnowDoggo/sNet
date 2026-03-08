using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class ServerPackage : Package
{
	public static readonly Event PlayerJoin = new Event();
	public static readonly Event PlayerQuit = new Event();
	
	public static readonly ReadOnlyTable Exports = new Dictionary<Obj, Obj>()
	{
		{ "playerJoin", PlayerJoin },
		{ "playerQuit", PlayerQuit },
	}.ToFrozenDictionary();

	public override string Name => "Server";
	public override Obj Export => Exports;
}