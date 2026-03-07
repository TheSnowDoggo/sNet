using sNet.Server;

namespace sNet.Service;

public abstract class ServerService : ServiceBase
{
	public NetServer Server { get; set; }
	
	public virtual void Receive(ServerNetCall call)
	{
	}

	public virtual void ClientJoined(RemoteClient client)
	{
	}

	public virtual void ClientLeft(RemoteClient client)
	{
	}
	
	protected async Task<bool> BroadcastPackAsync(byte sid, INetPackage data)
	{
		if (data.IsEmpty) return false;
		return await Server.BroadcastAsync(Format, (sid, (INetSerializable)data));
	}

	protected async Task<bool> SendPackAsync(int idx, byte sid, INetPackage data)
	{
		if (data.IsEmpty) return false;
		return await Server.SendAsync(idx, Format, (sid, (INetSerializable)data));
	}

	protected async Task<bool> SendAsync<T>(int idx, byte sid, T data)
		where T : INetSerializable
	{
		return await Server.SendAsync(idx, Format, (sid, data));
	}
}