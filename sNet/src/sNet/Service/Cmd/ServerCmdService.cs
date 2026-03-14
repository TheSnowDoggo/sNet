using System.Collections.Frozen;
using System.Diagnostics;
using sNet.Auth;
using sNet.Server;
using sNet.Service.Chat;

namespace sNet.Service.Cmd;

public sealed class ServerCmdService : ServerService
{
	public readonly FrozenDictionary<string, Cmd> Cmds;

	public ServerCmdService()
	{
		Cmds = new Dictionary<string, Cmd>
		{
			{ "shutdown", Cmd.Create(ShutdownCmd, 0, Permission.Admin) },
			{ "restart", Cmd.Create(RestartCmd, 0, Permission.Admin) },
			{ "login", Cmd.Create(LoginCmd, 2, Permission.None, true) },
			{ "chat", Cmd.Create(ChatCmd, 2, -1, Permission.None) },
			{ "broadcast", Cmd.Create(BroadcastCmd, 1, -1, Permission.Admin) },
		}.ToFrozenDictionary();
	}

	public int MaxReceiveSize { get; set; } = 512;
	
	public override ServiceId ServiceId => ServiceId.Cmd;

	public override void Receive(ServerNetCall call)
	{
		var sid = (CmdSid)call.Stream.ReadExactByte();

		switch (sid)
		{
		case CmdSid.RequestRun:
			HandleRequestRun(call);
			break;
		default:
			Logger.Error($"Unrecognised cmd sid: {sid}.");
			break;
		}
	}

	public bool TryRun(string input, RemoteClient client = null)
	{
		input = input.Trim();
		
		if (string.IsNullOrEmpty(input))
		{
			Logger.Error("Input was empty.");
			return false;
		}
		
		string[] args = Cmd.SplitArgs(input);

		if (!Cmds.TryGetValue(args[0], out Cmd cmd))
		{
			Logger.Error($"No command with name {args[0]} found.");
			return false;
		}

		return cmd.TryInvoke(args, client);
	}

	private async Task<bool> SendResponseAsync(int idx, string response)
	{
		return await SendAsync(idx, (byte)CmdSid.SendResponse, new SerialString(response));
	}

	private void HandleRequestRun(ServerNetCall call)
	{
		// Silently reject as only a malicious client could do this.
		if (call.Stream.Remaining() > MaxReceiveSize)
		{
			return;
		}
		
		var input = call.Stream.ReadNetUtf8();

		var response = TryRun(input, call.Client) ? Logger.LastInfo : Logger.LastError;
		
		Task.Run(() => SendResponseAsync(call.Client.Idx, response));
	}
	
	private bool ShutdownCmd(string[] args, RemoteClient client)
	{
		Server.Shutdown();

		_ = new Timer(_ => Environment.Exit(0), null, TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
		
		return true;
	}

	private bool RestartCmd(string[] args, RemoteClient client)
	{
		Server.Shutdown();

		var filename = Environment.ProcessPath;

		if (filename == null)
		{
			Logger.Error("Process path was null.");
			return false;
		}

		var psi = new ProcessStartInfo()
		{
			FileName = filename,
			UseShellExecute = false,
		};
		
		Process.Start(psi);
		return true;
	}

	private static bool LoginCmd(string[] args, RemoteClient client)
	{
		if (UserStore.Current == null)
		{
			Logger.Error("Login is not available.");
			return false;
		}
		
		var username = args[1];
		var password = args[2];

		if (!UserStore.Current.TryLogin(username, password, out User user))
		{
			return false;
		}
		
		client.Grant(user.Permissions);
		Logger.Info($"{client} login successful, permissions {user.Permissions} granted.");
		
		return true;
	}

	private bool ChatCmd(string[] args, RemoteClient client)
	{
		if (!Server.Services.TryGet<ServerChatService>(ServiceId.Chat, out var chatService))
		{
			Logger.Error("Chat Service is not available.");
			return false;
		}

		if (!int.TryParse(args[1], out var idx))
		{
			Logger.Error($"Client idx (\'{args[1]}\') was invalid.");
			return false;
		}

		if (!Server.Clients.HasClient(idx))
		{
			Logger.Error($"No client with idx {idx} found.");
			return false;
		}
		
		var message = string.Join(' ', args, 2, args.Length - 2);
		
		chatService.FireChat(idx, message);

		return true;
	}

	private bool BroadcastCmd(string[] args, RemoteClient client)
	{
		if (!Server.Services.TryGet<ServerChatService>(ServiceId.Chat, out var chatService))
		{
			Logger.Error("Chat Service is not available.");
			return false;
		}
		
		var message = string.Join(' ', args, 1, args.Length - 1);
		
		chatService.FireBroadcast(message);

		return true;
	}
}