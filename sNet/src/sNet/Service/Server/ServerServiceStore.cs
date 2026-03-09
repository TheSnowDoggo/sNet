using System.Collections;
using sNet.Server;

namespace sNet.Service;

public sealed class ServerServiceStore : IReadOnlyCollection<ServerService>
{
	private readonly Dictionary<ServiceId, ServerService> _services = [];
	
	private readonly NetServer _server;

	public ServerServiceStore(NetServer server)
	{
		_server = server;
	}
	
	public int Count => _services.Count;
	
	public ServerService this[ServiceId serviceId] => _services[serviceId];

	public ServerService Get(ServiceId serviceId)
	{
		return _services[serviceId];
	}

	public T Get<T>(ServiceId serviceId)
		where T : ServerService
	{
		return (T)_services[serviceId];
	}

	public bool TryGet(ServiceId id, out ServerService service)
	{
		return _services.TryGetValue(id, out service);
	}

	public bool TryGet<T>(ServiceId id, out T service)
		where T : ServerService
	{
		if (_services.TryGetValue(id, out var serverService))
		{
			service = (T)serverService;
			return true;
		}
		
		service = null;
		return false;
	}

	public void Add(ServerService service)
	{
		if (!_services.TryAdd(service.ServiceId, service))
		{
			throw new InvalidOperationException($"Service with id {service.ServiceId} already exists");
		}
		
		service.Server = _server;
		service.Initialize();
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

	public void ClientJoined(RemoteClient client)
	{
		foreach (var service in _services.Values)
		{
			service.ClientJoined(client);
		}
	}

	public void ClientLeft(RemoteClient client)
	{
		foreach (var service in _services.Values)
		{
			service.ClientLeft(client);
		}
	}

	public void Clear()
	{
		_services.Clear();
	}

	public IEnumerator<ServerService> GetEnumerator()
	{
		return _services.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}