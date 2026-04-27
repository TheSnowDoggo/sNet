using SCENeo;
using sNet;
using sNet.Auth;
using sNet.CScriptPro;
using sNet.Server;
using sNet.Service.Asset;
using sNet.Service.Chat;
using sNet.Service.Cmd;
using sNet.Service.Part;

namespace sNetServer;

public sealed class Server
{
	private const string ServerDirectory = "server";
	private const string AssetDirectory = "assets";
	private const string UsersFile = "users.json";
	
	private readonly Updater _updater;
	private Thread _updateThread;
	
	private readonly ServerPartRoot _root = new();
	private readonly ServerPartService _partService = new();
	private readonly ServerAssetService _assetService = new();
	private readonly ServerCmdService _cmdService = new();
	
	private NetServer _server;
	private ServerPackage _serverPackage;
	
	public Server()
	{
		_updater = new Updater(Update);
		
		_root.Service = _partService;
		_partService.Root = _root;
	}
	
	public bool Run()
	{
		if (!ServerConfig.TryLoadOrCreate("server_config.json", out var config))
		{
			return false;
		}
		
		SetupServer(config);

		_updater.FrameCap = config.UpdateFPSCap;

		_serverPackage = new ServerPackage(_server);
		
		PackageManager.Default.Packages.Add("Server", _serverPackage);
		
		if (!_server.Bind())
		{
			return false;
		}
		
		Logger.Info($"Server started at {_server.EndPointName}");

		if (!_server.Start())
		{
			return false;
		}

		if (!UserStore.TryLoadOrCreate(UsersFile, out var users))
		{
			return false;
		}

		UserStore.Current = users;

		Directory.CreateDirectory(AssetDirectory);

		if (!AssetIndex.TryDiscover(AssetDirectory, out var index))
		{
			return false;
		}
		
		_assetService.Index = index;
		
		Logger.Info($"Discovered {index.Assets.Length} assets.");

		LoadServerParts();

		Script.RunScripts(_root.Root, ScriptFlag.Server);

		_updateThread = new Thread(_updater.Start);
		_updateThread.Start();

		while (_server.Active)
		{
			var input = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(input))
			{
				continue;
			}
			
			_cmdService.TryRun(input);
		}
		
		return true;
	}

	private void SetupServer(ServerConfig config)
	{
		_server = NetServer.FromConfig(config);
		
		_server.Services.Add(new ServerChatService());
		_server.Services.Add(_partService);
		_server.Services.Add(_assetService);
		_server.Services.Add(_cmdService);
		
		_server.ClientJoined += Server_OnClientJoined;
		_server.ClientLeft += Server_OnClientLeft;
	}

	private void Server_OnClientJoined(RemoteClient client)
	{
		_serverPackage.PlayerJoin.Fire(client.Idx);
	}
	
	private void Server_OnClientLeft(RemoteClient client)
	{
		_serverPackage.PlayerQuit.Fire(client.Idx);
	}

	private void Update(double delta)
	{
		if (!_server.Active)
		{
			_updater.Stop();
			return;
		}
		
		_root.Update(delta);
	}

	private void LoadServerParts()
	{
		Directory.CreateDirectory(ServerDirectory);

		int parts = 0;
		
		foreach (var file in FileUtils.EnumerateFilesRecursive(ServerDirectory))
		{
			if (Path.GetExtension(file) != ".part")
			{
				continue;
			}
			
			if (!PartLoader.Default.TryGet(file, out var rootTag))
			{
				continue;
			}

			if (rootTag.Flags.HasFlag(PartFlag.NoAuto))
			{
				continue;
			}

			if (!rootTag.Flags.HasFlag(PartFlag.Root))
			{
				_root.Root.AddChild(rootTag.Create());
				parts++;
				continue;
			}
			
			foreach (var child in rootTag.Children)
			{
				_root.Root.AddChild(child.Create());
				parts++;
			}
		}
		
		Logger.Info($"Successfully loaded {parts} parts.");
	}
}