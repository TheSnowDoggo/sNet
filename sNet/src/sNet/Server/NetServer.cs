using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using sNet.Service;

namespace sNet.Server;

public sealed class NetServer
{
	private sealed class ClientInfo
	{
		private int _isReceiving;

		public ClientInfo(RentBuffer buffer)
		{
			Buffer = buffer;
		}

		public RentBuffer Buffer { get; }
		
		public NetCall NetCall { get; set; }
		
		public bool BeginReceiving()
		{
			return Interlocked.CompareExchange(ref _isReceiving, 1, 0) == 0;
		}

		public void EndReceiving()
		{
			Interlocked.Exchange(ref _isReceiving, 0);
		}
	}

	private readonly ConcurrentQueue<Socket> _joinQueue = [];
	private readonly ConcurrentQueue<int>    _quitQueue = [];

	private readonly ClientInfo[] _clientInfo;

	private int _isAccepting;
	private int _isActive;
	
	private Socket _socket;
	private Thread _processThread;

	public NetServer(int maxClients)
	{
		Clients = new ClientStore(maxClients);
		_clientInfo = new ClientInfo[maxClients];
		Services = new ServerServiceStore(this);
	}

	public int Port { get; set; } = 17324;
	public int Backlog { get; set; } = 10;
	public int MaxReceiveSize { get; init; } = int.MaxValue;

	public event Action<RemoteClient> ClientJoined;
	public event Action<RemoteClient> ClientLeft;

	public bool Active => _isActive != 0;

	public ClientStore Clients { get; }
	public ServerServiceStore Services { get; }

	public string EndPointName => _socket?.LocalEndPoint?.ToString();
	
	public bool Bind()
	{
		try
		{
			var endPoint = new IPEndPoint(IPAddress.Any, Port);

			_socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			_socket.Bind(endPoint);

			return true;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			
			return false;
		}
	}
	
	public bool Start()
	{
		if (Interlocked.Exchange(ref _isActive, 1) != 0)
		{
			Logger.Error("Server is alreay running.");
			return false;
		}
		
		try
		{
			_socket.Listen(Backlog);
			
			_processThread = new Thread(Process);
			_processThread.Start();
			
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			
			Interlocked.Exchange(ref _isActive, 0);
			
			return false;
		}
	}

	public async Task<bool> SendAsync(int idx, RentBuffer buffer)
	{
		if (!Active)
		{
			Logger.Error("Server is not running.");
			return false;
		}
		
		if (!Clients.TryGetClient(idx, out var client))
		{
			Logger.Error($"No such client {idx} found.");
			return false;
		}
		
		try
		{
			int sent = await client.Socket.SendAsync(buffer);
			return sent != buffer.End;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
	}

	public async Task<bool> BroadcastAsync(RentBuffer buffer, HashSet<int> exclude = null)
	{
		try
		{
			var tasks = new List<Task>();
		
			for (int i = 0; i < Clients.ActiveClients.Count; i++)
			{
				int idx = Clients.ActiveClients[i];

				if (exclude != null && exclude.Contains(idx))
				{
					continue;
				}
				
				tasks.Add(SendAsync(idx, buffer));
			}
		
			await Task.WhenAll(tasks.ToArray());
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
	}

	public bool Kick(int idx)
	{
		if (!Clients.HasClient(idx))
		{
			Logger.Error($"Client {idx} was not found.");
			return false;
		}

		_quitQueue.Enqueue(idx);
		return true;
	}

	public bool Shutdown()
	{
		try
		{
			_socket.Close();
			_socket = null;
			
			return Interlocked.CompareExchange(ref _isActive, 0, 1) == 0;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
	}

	private void Process()
	{
		while (Active)
		{
			if (Interlocked.CompareExchange(ref _isAccepting, 1, 0) == 0)
			{
				Task.Run(AcceptAsync);
			}

			ProcessQuits();
			ProcessJoins();
			ProcessReceives();
		}

		ProcessEnd();
	}

	private async Task AcceptAsync()
	{
		Socket client = await _socket.AcceptAsync();

		Interlocked.Exchange(ref _isAccepting, 0);
		
		if (Clients.Count >= Clients.MaxClients)
		{
			Logger.Info($"Client {client.RemoteEndPoint} was rejected as server was full.");
			return;
		}
		
		_joinQueue.Enqueue(client);
	}
	
	private void ProcessQuits()
	{
		var seen = new HashSet<int>();
		
		while (_quitQueue.TryDequeue(out int idx))
		{
			if (!seen.Add(idx))
			{
				continue;
			}

			if (!Clients.TryGetClient(idx, out var client))
			{
				Logger.Error($"Client {idx} was not found.");
				continue;
			}
			
			try
			{
				string clientStr = client.ToString();
				
				Services.ClientLeft(client);
				ClientLeft?.Invoke(client);
				
				Clients.Remove(idx);
					
				var info = _clientInfo[idx];
				info.Buffer.Dispose();
				info.NetCall?.Dispose();
				
				_clientInfo[idx] = null;
				
				Logger.Info($"Client {clientStr} quit.");
			}
			catch (Exception ex)
			{
				Logger.Error($"Failed to kick client {idx}: {ex.Message}");
			}
		}
	}
	
	private void ProcessJoins()
	{
		while (_joinQueue.TryDequeue(out var clientSocket))
		{
			if (Clients.Count >= Clients.MaxClients)
			{
				Logger.Error($"Tried to add client at {clientSocket.RemoteEndPoint} but server was full.");
				break;
			}
			
			try
			{
				var client = new RemoteClient(clientSocket);

				Clients.Add(client);
				_clientInfo[client.Idx] = new ClientInfo(RentBuffer.Share(Constants.BufferSize));
				
				Services.ClientJoined(client);
				ClientJoined?.Invoke(client);
			}
			catch (Exception ex)
			{
				Logger.Error($"Failed to add client: {ex.Message}");
			}
		}
	}

	private void ProcessReceives()
	{
		foreach (var client in Clients)
		{
			var info = _clientInfo[client.Idx];

			if (!info.BeginReceiving())
			{
				continue;
			}

			new TaskFactory().StartNew(ProcessReceive, client);
		}
	}

	private async Task ProcessReceive(object state)
	{
		if (state == null)
		{
			Logger.Error("Received null state.");
			return;
		}
		
		var client = (RemoteClient)state;
		var info = _clientInfo[client.Idx];

		try
		{
			if (!client.Socket.Connected)
			{
				_quitQueue.Enqueue(client.Idx);
				return;
			}

			int received = await client.Socket.ReceiveAsync(info.Buffer);

			if (received < 0)
			{
				_quitQueue.Enqueue(client.Idx);
				return;
			}

			using var stream = info.Buffer.OpenRead();

			while (stream.Position < received)
			{
				if (info.NetCall == null)
				{
					int messageSize = stream.ReadNetInt32();

					if (messageSize <= 0 || messageSize > MaxReceiveSize)
					{
						Logger.Error($"Message size {messageSize} was invalid.");
						_quitQueue.Enqueue(client.Idx);
						break;
					}

					info.NetCall = new ServerNetCall(client, RentBuffer.Share(messageSize));
				}

				// Smaller betweeen: Total remaining bytes AND Remaining bytes in stream
				int count = Math.Min(info.NetCall.End - (int)info.NetCall.Stream.Position,
					received - (int)stream.Position);

				for (int i = 0; i < count; i++)
				{
					info.NetCall.Stream.WriteByte((byte)stream.ReadByte());
				}

				if (info.NetCall.Stream.Position < info.NetCall.End)
				{
					continue;
				}

				_ = new TaskFactory().StartNew(RunCallback, info.NetCall);
				info.NetCall = null;
			}
		}
		catch (Exception ex)
		{
			info.NetCall?.Dispose();

			if (!client.Socket.Connected)
			{
				_quitQueue.Enqueue(client.Idx);
				return;
			}

			Logger.Error(ex.Message);
		}
		finally
		{
			info.EndReceiving();
		}
	}

	private void RunCallback(object state)
	{
		if (state == null)
		{
			Logger.Error("Received null state.");
			return;
		}
		
		using var call = (ServerNetCall)state;

		try
		{
			call.Stream.Seek(0, SeekOrigin.Begin);
			
			Services.Receive(call);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
		}
	}

	private void ProcessEnd()
	{
		try
		{
			foreach (var client in Clients)
			{
				client.Socket.Close();
			}
			
			Clients.Clear();
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to end process: {ex.Message}");
		}
	}
}