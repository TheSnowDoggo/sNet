using sNet.Client;

namespace sNet.Service;

public abstract class ClientService
{
	public NetClient Client { get; set; }
	public abstract ServiceId ServiceId { get; }

	public virtual void Initialize()
	{
	}
	
	public virtual void Receive(NetCall call)
	{
	}
}