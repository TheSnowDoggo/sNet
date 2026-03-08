using System.Collections;

namespace sNet.Server;

public sealed class ClientStore : IReadOnlyList<RemoteClient>
{
	private readonly RemoteClient[] _clientData;
	private readonly List<int> _activeClients;
	
	public ClientStore(int maxClients)
	{
		_clientData = new RemoteClient[maxClients];
		_activeClients = new List<int>(maxClients);
	}
	
	public int MaxClients => _clientData.Length;
	
	public int Count { get; private set; }
	
	public IReadOnlyList<int> ActiveClients => _activeClients.AsReadOnly();
	
	public RemoteClient this[int index] => _clientData[index];

	public bool HasClient(int idx)
	{
		return idx >= 0 && idx < _activeClients.Count && _clientData[idx] != null;
	}
	
	public bool TryGetClient(int idx, out RemoteClient client)
	{
		if (idx < 0 || idx >= MaxClients)
		{
			client = null;
			return false;
		}
		
		client = _clientData[idx];
		return client != null;
	}
	
	public void Add(RemoteClient client)
	{
		if (Count >= MaxClients)
		{
			throw new InvalidOperationException($"Maximum number of clients {MaxClients} exceeded");
		}

		int idx = ResolveNextIdx();
		_clientData[idx] = client;
		_activeClients.Add(idx);
		client.Idx = idx;

		Count++;
	}

	public void Remove(int idx)
	{
		if (idx < 0 || idx >= MaxClients)
		{
			throw new IndexOutOfRangeException($"Client idx {idx} is invalid");
		}

		if (_clientData[idx] == null)
		{
			throw new InvalidOperationException($"No such client with idx {idx} found.");
		}

		if (!_activeClients.Remove(idx))
		{
			throw new InvalidOperationException($"Failed to remove client {idx}.");
		}
		
		var client = _clientData[idx];
		
		client.Idx = -1;
		
		_clientData[idx] = null;
		
		Count--;
		
	}

	public void Clear()
	{
		Array.Clear(_clientData);
		_activeClients.Clear();
		Count = 0;
	}

	private int ResolveNextIdx()
	{
		for (int i = 0; i < MaxClients; i++)
		{
			if (_clientData[i] == null)
			{
				return i;
			}
		}

		throw new InvalidOperationException("Failed to resolve idx.");
	}

	public IEnumerator<RemoteClient> GetEnumerator()
	{
		for (int i = 0; i < _activeClients.Count; i++)
		{
			yield return _clientData[_activeClients[i]];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}