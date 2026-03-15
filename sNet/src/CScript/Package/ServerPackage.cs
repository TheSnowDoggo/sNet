using System.Collections.Frozen;
using sNet.Server;

namespace sNet.CScriptPro;

public sealed class ServerPackage : Package
{
	private readonly ReadOnlyTable _export;
	
	public readonly Event PlayerJoin = new Event();
	public readonly Event PlayerQuit = new Event();

	public ServerPackage(NetServer server)
	{
		Server = server;
		
		_export = new Dictionary<Obj, Obj>()
		{
			{ "playerJoin", PlayerJoin },
			{ "playerQuit", PlayerQuit },
			{ "getPlayers", GlobalFunction.Create(GetPlayers) },
		}.ToFrozenDictionary();
	}
	
	public NetServer Server { get; }

	public override string Name => "Server";
	public override ReadOnlyTable Export => _export;

	private ArrayBaseObj GetPlayers(Obj[] args)
	{
		return new ArrayViewObj<Number>([..Server.Clients.ActiveClients]);
	}
}