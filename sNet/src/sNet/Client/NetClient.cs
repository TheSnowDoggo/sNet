using System.Net;
using System.Net.Sockets;

namespace sNet.Client;

public sealed class NetClient
{
	private Socket  _socket;
	private NetCall _call;

	public NetClient()
	{
		Services = new ClientServiceStore(this);
	}
	
	public int MaxReceiveSize { get; init; } = int.MaxValue;
	
	public ClientServiceStore Services { get; }
	
	public bool Connected => _socket is { Connected: true };

	public async Task<bool> ConnectAsync(IPAddress address, int port)
	{
		if (Connected)
		{
			Logger.Error("Client is already connected.");
			return false;
		}

		try
		{
			var endPoint = new IPEndPoint(address, port);

			_socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			await _socket.ConnectAsync(endPoint);

			return true;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			
			return false;
		}
	}

	public async Task<bool> ConnectAsync(string hostname, int port)
	{
		try
		{
			var addresses = await Dns.GetHostAddressesAsync(hostname);
			
			return await ConnectAsync(addresses[0], port);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
	}

	public async Task<bool> DisconnectAsync()
	{
		if (!Connected)
		{
			Logger.Error("Cannot disconnect client as it's not connected.");
			return false;
		}
		
		try
		{
			await _socket.DisconnectAsync(false);
			_socket.Close();
			_socket = null;
			
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
		if (!Connected)
		{
			Logger.Error("Cannot start client as it's not connected.");
			return false;
		}
		
		try
		{
			Task.Run(Run);
			
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
	}

	public async Task<bool> SendAsync(RentBuffer buffer)
	{
		if (!Connected)
		{
			Logger.Error("Cannot send client as it's not connected.");
			return false;
		}
		
		try
		{
			return await _socket.SendAsync(buffer) == buffer.End;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
	}

	private async Task Run()
	{
		using var buffer = RentBuffer.Share(Constants.BufferSize);
		
		while (Connected)
		{
			await ReceiveAsync(buffer);
		}
		
		_call?.Dispose();
		_call = null;
	}

	private async Task ReceiveAsync(RentBuffer buffer)
	{
		try
		{
			int received = await _socket.ReceiveAsync(buffer);

			if (received < 0)
			{
				await DisconnectAsync();
				return;
			}

			using var stream = buffer.OpenRead();

			while (stream.Position < received)
			{
				if (_call == null)
				{
					int bytes = stream.ReadNetInt32();

					if (bytes < 0 || bytes > MaxReceiveSize)
					{
						Logger.Error($"Received byte count {bytes} is invalid.");
						return;
					}

					_call = new NetCall(RentBuffer.Share(bytes));
				}

				int count = Math.Min(_call.End - (int)_call.Stream.Position, received - (int)stream.Position);

				for (int i = 0; i < count; i++)
				{
					_call.Stream.WriteByte((byte)stream.ReadByte());
				}

				if (_call.Stream.Position < _call.End)
				{
					continue;
				}

				_ = new TaskFactory().StartNew(RunCallback, _call);
				_call = null;
			}
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
		}
	}
	
	private void RunCallback(object state)
	{
		if (state == null)
		{
			Logger.Error("Received null state.");
			return;
		}
		
		using var call = (NetCall)state;

		try
		{
			call.Stream.Seek(0, SeekOrigin.Begin);
			
			Services.Receive(call);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
		}
	}
}