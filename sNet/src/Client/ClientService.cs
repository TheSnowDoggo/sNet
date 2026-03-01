namespace sNet.Client;

public abstract class ClientService
{
	public ClientService(ServiceId serviceId)
	{
		ServiceId = serviceId;
	}
	
	public NetClient Client { get; set; }
	public ServiceId ServiceId { get; }

	public virtual void Receive(NetCall call)
	{
	}
}