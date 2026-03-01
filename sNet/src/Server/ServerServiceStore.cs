using System.Collections;

namespace sNet.Server;

public sealed class ServerServiceStore : IReadOnlyCollection<IServerService>
{
	private readonly Dictionary<ServiceId, IServerService> _services = [];
	
	private readonly NetServer _server;

	public ServerServiceStore(NetServer server)
	{
		_server = server;
	}
	
	public int Count => _services.Count;
	
	public IServerService this[ServiceId serviceId] => _services[serviceId];

	public IServerService Get(ServiceId serviceId)
	{
		return _services[serviceId];
	}

	public T Get<T>(ServiceId serviceId)
		where T : IServerService
	{
		return (T)_services[serviceId];
	}

	public void Add(IServerService service)
	{
		if (!_services.TryAdd(service.ServiceId, service))
		{
			throw new InvalidOperationException($"Service with id {service.ServiceId} already exists");
		}
		
		service.Server = _server;
	}

	public void Remove(ServiceId serviceId)
	{
		if (!_services.TryGetValue(serviceId, out var service))
		{
			throw new InvalidOperationException($"Service with id {serviceId} does not exist");
		}
		
		service.Server = null;
		_services.Remove(serviceId);
	}

	public void Receive(ServerNetCall call)
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

	public IEnumerator<IServerService> GetEnumerator()
	{
		return _services.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}