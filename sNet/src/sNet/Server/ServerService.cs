namespace sNet.Server;

public abstract class ServerService
{
	public NetServer Server { get; set; }
	
	public abstract ServiceId ServiceId { get; }

	public virtual void Initialize()
	{
	}

	public virtual void Receive(ServerNetCall call)
	{
	}

	public virtual void ClientJoined(RemoteClient client)
	{
	}

	public virtual void ClientLeft(RemoteClient client)
	{
	}
}