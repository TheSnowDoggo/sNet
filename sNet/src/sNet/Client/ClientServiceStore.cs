using System.Collections;

namespace sNet.Client;

public sealed class ClientServiceStore : IReadOnlyCollection<ClientService>
{
	private readonly NetClient _client;
	
	private readonly Dictionary<ServiceId, ClientService> _services = [];

	public ClientServiceStore(NetClient client)
	{
		_client = client;
	}
	
	public int Count => _services.Count;
	
	public ClientService this[ServiceId serviceId] => _services[serviceId];

	public ClientService Get(ServiceId serviceId)
	{
		return _services[serviceId];
	}

	public T Get<T>(ServiceId serviceId)
		where T : ClientService
	{
		return (T)_services[serviceId];
	}
	
	public void Add(ClientService service)
	{
		if (!_services.TryAdd(service.ServiceId, service))
		{
			throw new InvalidOperationException($"Service with id {service.ServiceId} already exists");
		}
		
		service.Client = _client;
		service.Initialize();
	}

	public void Remove(ServiceId serviceId)
	{
		if (!_services.TryGetValue(serviceId, out var service))
		{
			throw new InvalidOperationException($"Service with id {serviceId} does not exist");
		}
		
		service.Client = null;
		_services.Remove(serviceId);
	}
	
	public void Receive(NetCall call)
	{
		var serviceId = (ServiceId)call.Stream.ReadExactByte();
		
		if (!_services.TryGetValue(serviceId, out var service))
		{
			Logger.Error($"Unrecognized service with id {serviceId}");
			return;
		}
		
		try
		{
			service.Receive(call);
		}
		catch (Exception ex)
		{
			Logger.Error($"During receive for {serviceId}: {ex.Message}");
		}
	}

	public void Clear()
	{
		_services.Clear();
	}

	public IEnumerator<ClientService> GetEnumerator()
	{
		return _services.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}