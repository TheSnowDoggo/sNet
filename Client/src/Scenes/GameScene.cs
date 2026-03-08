using SCENeo;
using SCENeo.Scenes;
using sNet;
using sNet.CScriptPro;
using sNet.Service.Part;

namespace sNetClient;

public sealed class GameScene : Scene
{
	private readonly ClientPartRoot _root = new ClientPartRoot();
	
	private readonly RenderChannel _renderChannel;
	private readonly RenderEngine _renderEngine;
	
	public GameScene()
	{
		_renderChannel = new RenderChannel()
		{
			BasePixel = Pixel.DarkCyan,
		};

		_renderEngine = new RenderEngine()
		{
			Root = _root.Root,
		};

		_renderEngine.Channels.Add(0, _renderChannel);
	}

	public override IEnumerable<IRenderable> Render()
	{
		_renderEngine.Render();
		return [_renderChannel];
	}

	public override void Start()
	{
		var partService = Client.Instance.Net.Services.Get<ClientPartService>(ServiceId.Part);
		
		partService.Root = _root;
		_root.Service = partService;
		
		PackageManager.Default.Packages.Add("Input", new InputPackage());
		
		Client.Instance.Net.Disconnected += Client_OnDisconnected;
	}

	public override void Open()
	{
		base.Open();
		Client.Instance.Chat.Open();
		Client.Instance.Pause.Open();
	}

	public override void Close()
	{
		base.Close();
		Client.Instance.Chat.Close();
		Client.Instance.Pause.Close();
	}

	public override void Update(double delta)
	{
		_root.Update(delta);
	}

	public override void UnfocusedInput(ConsoleKeyInfo cki)
	{
		InputPackage.InputEvent.Fire((int)cki.Key);
	}

	public override void DisplayResize(Vec2I size)
	{
		_renderChannel.Width = size.X;
		_renderChannel.Height = size.Y;
	}
	
	private void Client_OnDisconnected()
	{
		Client.Instance.Alert.Alert("Disconnected from the server.");
		ChangeTo(Client.Instance.Connect);

		Event.Update.Clear();

		ImageLoader.Default = new ImageLoader();
		ScriptLoader.Default = new ScriptLoader();
		
		_root.Clear();
	}
}