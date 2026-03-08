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
	
	private readonly Updater _updater;
	private Thread _updateThread;
	
	private NetServer _server;
	private ServerPackage _serverPackage;
	private readonly ServerPartRoot _root = new ServerPartRoot();
	private readonly ServerPartService _partService = new ServerPartService();
	private readonly ServerAssetService _assetService = new ServerAssetService();
	private readonly ServerCmdService _cmdService = new ServerCmdService();

	public Server()
	{
		_updater = new Updater(Update)
		{
			FrameCap = 60,
		};
		
		_root.Service = _partService;
		_partService.Root = _root;
	}
	
	public bool Run()
	{
		if (!ServerConfig.TryLoadOrCreate("server_config.json", out var config))
		{
			return false;
		}

		_server = NetServer.FromConfig(config);
		
		_server.Services.Add(new ServerChatService());
		_server.Services.Add(_partService);
		_server.Services.Add(_assetService);
		_server.Services.Add(_cmdService);
		
		_server.ClientJoined += Server_OnClientJoined;
		_server.ClientLeft += Server_OnClientLeft;

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

		if (!UserStore.TryLoadOrCreate("users.json", out var users))
		{
			return false;
		}

		UserStore.Current = users;

		Directory.CreateDirectory("assets");

		if (!AssetIndex.TryDiscover("assets", out var index))
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
			
			try
			{
				var tags = PartTag.ParseAll(file);

				foreach (var tag in tags)
				{
					_root.Root.AddChild(tag.Create());

					parts++;
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Failed to load parts from {file}: {ex.Message}");
			}
		}
		
		Logger.Info($"Successfully loaded {parts} parts.");
	}
}