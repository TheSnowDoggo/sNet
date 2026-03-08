using SCENeo;
using SCENeo.Scenes;
using SCENeo.Ui;
using SCEWin;
using sNet.Client;
using sNet.Service.Asset;
using sNet.Service.Chat;
using sNet.Service.Cmd;
using sNet.Service.Part;

namespace sNetClient;

public class Client
{
	private readonly Display _display;

	public Client()
	{
		Updater = new Updater(Update)
		{
			FrameCap = 60,
		};
		
		Viewport = new Viewport()
		{
			BasePixel = Pixel.DarkGray,
		};
		
		_display = new Display(Display_OnResize)
		{
			Renderable = Viewport,
		#pragma warning disable CA1416
			Output = Environment.OSVersion.Platform == PlatformID.Win32NT ? WinOutput.Instance : ConsoleOutput.Instance,
		#pragma warning restore CA1416
		};

		Scenes = [Log, Game, Connect, Alert, Loading, Chat, Pause];

		var services = Net.Services;
		
		services.Add(new ClientChatService());
		services.Add(new ClientPartService());
		services.Add(new ClientAssetService());
		services.Add(new ClientCmdService());
	}

	public static Client Instance { get; } = new Client();
	
	public readonly Viewport Viewport;
	
	public readonly Updater Updater;
	
	public readonly LogScene Log  = new LogScene();
	public readonly GameScene Game = new GameScene();
	public readonly ConnectScene Connect = new ConnectScene();
	public readonly AlertScene Alert = new AlertScene();
	public readonly LoadingScene Loading = new LoadingScene();
	public readonly ChatScene Chat = new ChatScene();
	public readonly PauseScene Pause = new PauseScene();
	
	public readonly SceneManager Scenes;
	
	public readonly NetClient Net = new NetClient();

	public void Run()
	{
		Scenes.Start();
		Updater.Start();
	}

	private void Update(double delta)
	{
		while (Console.KeyAvailable)
		{
			Scenes.UnfocusedInput(Console.ReadKey(true));
		}
		
		Scenes.Update(delta);

		Viewport.Source = Scenes.Render();
		
		_display.Update();
	}

	private void Display_OnResize(Vec2I size)
	{
		Viewport.Width = size.X;
		Viewport.Height = size.Y;
		
		Scenes.DisplayResize(size);
	}
}