namespace sNet.Server;

public abstract class IServerService
{
	public IServerService(ServiceId serviceId)
	{
		ServiceId = serviceId;
	}
	
	public NetServer Server { get; set; }
	
	public ServiceId ServiceId { get; }

	public virtual void Receive(ServerNetCall call)
	{
	}
}