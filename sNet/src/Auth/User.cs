using sNet.Server;

namespace sNet.Auth;

public sealed class User
{
	public string Password { get; set; }
	public Permission Permissions { get; set; }
}