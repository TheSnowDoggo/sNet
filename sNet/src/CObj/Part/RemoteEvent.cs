using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class RemoteEvent : Part
{
	public new static readonly FrozenDictionary<string, IProperty> GlobalProperties = new Dictionary<string, IProperty>(Part.GlobalProperties)
	{
		{ "fireAllClients", new GProperty<RemoteEvent>(p => GlobalFunction.Create(p.FireAllClients, 0, -1)) },
		{ "fireClient", new GProperty<RemoteEvent>(p => GlobalFunction.Create(p.FireClient, 1, -1, TypeId.Number)) },
		{ "fireServer", new GProperty<RemoteEvent>(p => GlobalFunction.Create(p.FireServer, 0, -1)) },
		{ "client", new GProperty<RemoteEvent>(p => p.Client) },
		{ "server", new GProperty<RemoteEvent>(p => p.Server) },
	}.ToFrozenDictionary();
	
	public Event Client { get; } = new Event();
	public Event Server { get; } = new Event();

	public override PartType PartType => PartType.RemoteEvent;

	public override IReadOnlyDictionary<string, IProperty> Properties => GlobalProperties;

	private void FireAllClients(Obj[] args)
	{
		Root.FireAllClients(this, args);
	}

	private void FireClient(Obj[] args)
	{
		int idx = (int)args[0];
		
		Root.FireClient(this, idx, args[1..]);
	}
	
	private void FireServer(Obj[] args)
	{
		Root.FireServer(this, args);
	}
}