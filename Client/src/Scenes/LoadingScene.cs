using SCENeo;
using SCENeo.Scenes;
using SCENeo.Ui;
using sNet;
using sNet.Service.Asset;

namespace sNetClient;

public sealed class LoadingScene : Scene
{
	private const int MaxAttempts = 3;
	
	private readonly TextLabel _info = new TextLabel()
	{
		Width = 30,
		Height = 1,
		Anchor = Anchor.Center | Anchor.Middle,
		Offset = Vec2I.Up * 2,
		Text = "<no_info>",
	};

	private readonly ProgressBar _swingyBar = new ProgressBar()
	{
		Width = 70,
		Height = 2,
		Min = -1,
		Max = 1,
		Anchor = Anchor.Center | Anchor.Middle,
	};

	private double _swingyTimer = 0;

	private ClientAssetService _assetService;

	public override void Start()
	{
		_assetService = Client.Instance.Net.Services.Get<ClientAssetService>(ServiceId.Asset);
		_assetService.AssetReceived += AssetService_OnAssetReceived;
	}
	
	public override void Update(double delta)
	{
		_swingyBar.Value = Math.Sin(_swingyTimer);
		_swingyTimer += delta;
	}

	public override IEnumerable<IRenderable> Render()
	{
		return [_info, _swingyBar];
	}

	public async Task Connect(string hostname, int port)
	{
		_info.Text = $"Connecting to {hostname}:{port}";
		_swingyBar.FillPixel = Pixel.Green;

		if (!await LoopConnect(hostname, port))
		{
			Client.Instance.Alert.Alert($"Exceed max connection attempts of {MaxAttempts} attempts", duration: 2.5);
			ChangeTo(Client.Instance.Connect);
			return;
		}
		
		_info.Text = "Connection successful!";

		if (!Client.Instance.Net.Start())
		{
			Client.Instance.Alert.Alert($"Failed to start net client: {Logger.LastError}");
			ChangeTo(Client.Instance.Connect);
		}

		_info.Text = "Requesting assets...";
		
		_swingyBar.FillPixel = Pixel.Blue;
	}

	private static async Task<bool> LoopConnect(string hostname, int port)
	{
		var client = Client.Instance.Net;
		
		for (int i = 0; i < MaxAttempts; i++)
		{
			if (await client.ConnectAsync(hostname, port))
			{
				return true;
			}
			
			Client.Instance.Alert.Alert(Logger.LastError, duration: 2.5);
			
			await Task.Delay(3000);
		}
		
		return false;
	}
	
	private void AssetService_OnAssetReceived(int assetIdx)
	{
		_info.Text = $"Assets received: {_assetService.Received}/{_assetService.AssetCount}";

		if (_assetService.Received >= _assetService.AssetCount)
		{
			ChangeTo(Client.Instance.Game);
		}
	}
}