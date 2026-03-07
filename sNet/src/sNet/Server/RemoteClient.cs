using System.Net.Sockets;

namespace sNet.Server;

public sealed class RemoteClient
{
	public RemoteClient(Socket socket)
	{
		Socket = socket;
	}
	
	public Socket Socket { get; }
	public int Idx { get; set; }

	public override string ToString()
	{
		return $"[{Idx}] {Socket.RemoteEndPoint}";
	}
}