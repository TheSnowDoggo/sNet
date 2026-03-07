using sNet.Client;

namespace sNet.Service;

public abstract class ClientService : ServiceBase
{
	public NetClient Client { get; set; }
	
	public virtual void Receive(NetCall call)
	{
	}
	
	protected async Task<bool> SendAsync<T>(byte sid, T data)
		where T : INetSerializable
	{
		return await Client.SendAsync(Format, (sid, data));
	}
}