using System.Collections.Frozen;
using System.Diagnostics;
using System.Text;
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
			{ "help", Cmd.Create(HelpCmd, 0, 0, Permission.None) },
			{ "shutdown", Cmd.Create(ShutdownCmd, 0, Permission.Admin) },
			{ "restart", Cmd.Create(RestartCmd, 0, Permission.Admin) },
			{ "kick", Cmd.Create(KickCmd, 1, 1, Permission.Admin) },
			{ "cinfo", Cmd.Create(ClientInfoCmd, 1, 1, Permission.Admin) },
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

		var call = new CmdCall()
		{
			Args = args,
			Client = client,
		};

		return cmd.TryInvoke(call);
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

	private bool HelpCmd(CmdCall call)
	{
		var cb = new ColumnBuilder()
		{
			Prefix = $"- Displaying {Cmds.Count} Commands -\n",
		};
		
		cb.AddRow("Name", "Args", "Permissions", "Remote");
		
		cb.AddHorizontalBorder();
		
		foreach ((string name, Cmd cmd) in Cmds)
		{
			cb.AddRow(name, cmd.GetFormatedArgRange(), cmd.Permissions, cmd.Remote);
		}
		
		Logger.Info(cb.ToString());
		
		return true;
	}
	
	private bool ShutdownCmd(CmdCall call)
	{
		Server.Shutdown();

		_ = new Timer(_ => Environment.Exit(0), null, TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
		
		return true;
	}

	private bool RestartCmd(CmdCall call)
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
	
	private bool KickCmd(CmdCall call)
	{
		if (!TryParseClientIdx(call.Args[1], out int idx))
		{
			return false;
		}

		if (!Server.Kick(idx))
		{
			Logger.Error("Kick failed.");
			return false;
		}
		
		Logger.Info($"Client {idx} kicked successfully.");
		
		return true;
	}

	private bool ClientInfoCmd(CmdCall call)
	{
		if (!TryParseClientIdx(call.Args[1], out int idx))
		{
			return false;
		}
		
		var client = Server.Clients[idx];

		var cb = new ColumnBuilder()
		{
			Prefix = $"- Displaying info for {idx} -\n",
		};

		cb.AddRow("Name", "Value");
		
		cb.AddHorizontalBorder();
		
		cb.AddRow("Local", $"{client.Socket.LocalEndPoint}");
		cb.AddRow("Remote", $"{client.Socket.RemoteEndPoint}");
		
		cb.AddRow("Permissions", $"{client.Permissions}");
		cb.AddRow("Join Time", $"{client.JoinTime}");
		
		Logger.Info(cb.ToString());
		
		return true;
	}
	
	private static bool LoginCmd(CmdCall call)
	{
		if (UserStore.Current == null)
		{
			Logger.Error("Login is not available.");
			return false;
		}
		
		var username = call.Args[1];
		var password = call.Args[2];

		if (!UserStore.Current.TryLogin(username, password, out User user))
		{
			return false;
		}
		
		call.Client.Grant(user.Permissions);
		Logger.Info($"{call.Client} login successful, permissions {user.Permissions} granted.");
		
		return true;
	}

	private bool ChatCmd(CmdCall call)
	{
		if (!Server.Services.TryGet<ServerChatService>(ServiceId.Chat, out var chatService))
		{
			Logger.Error("Chat Service is not available.");
			return false;
		}

		if (!TryParseClientIdx(call.Args[1], out int idx))
		{
			return false;
		}
		
		var message = string.Join(' ', call.Args, 2, call.Args.Length - 2);
		
		chatService.FireChat(idx, message);

		return true;
	}

	private bool BroadcastCmd(CmdCall call)
	{
		if (!Server.Services.TryGet<ServerChatService>(ServiceId.Chat, out var chatService))
		{
			Logger.Error("Chat Service is not available.");
			return false;
		}
		
		var message = string.Join(' ', call.Args, 1, call.Args.Length - 1);
		
		chatService.FireBroadcast(message);

		return true;
	}

	private bool TryParseClientIdx(string arg, out int idx)
	{
		if (!int.TryParse(arg, out idx))
		{
			Logger.Error($"Client idx (\'{arg}\') was invalid.");
			return false;
		}

		if (!Server.Clients.HasClient(idx))
		{
			Logger.Error($"No client with idx {idx} found.");
			return false;
		}

		return true;
	}
}