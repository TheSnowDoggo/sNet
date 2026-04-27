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
	public Permission Permissions { get; private set; }

	public DateTime JoinTime { get; init; }
	
	public override string ToString()
	{
		return $"[{Idx}] {Socket.RemoteEndPoint}";
	}

	public void Grant(Permission permissions)
	{
		Permissions |= permissions;
	}

	public void Revoke(Permission permissions)
	{
		Permissions &= ~permissions;
	}
	
	public bool AuthorisedAny(Permission permissions)
	{
		return (Permissions & permissions) != 0;
	}

	public bool AuthorisedAll(Permission permissions)
	{
		return (Permissions & permissions) == permissions;
	}
}