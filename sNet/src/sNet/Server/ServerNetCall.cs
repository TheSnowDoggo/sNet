using Serial;

namespace sNet.Server;

public sealed class ServerNetCall : NetCall
{
	public ServerNetCall(RemoteClient client, RentBuffer buffer)
		: base(buffer)
	{
		Client = client;
	}
	
	public RemoteClient Client { get; }
}