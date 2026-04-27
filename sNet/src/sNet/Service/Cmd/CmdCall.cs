using sNet.Server;

namespace sNet.Service.Cmd;

public sealed class CmdCall
{
	public string[] Args { get; init; }
	public RemoteClient Client { get; init; }
}