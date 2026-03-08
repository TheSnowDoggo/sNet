namespace sNet.Service.Asset;

public sealed class ClientAssetService : ClientService
{
	private int _isReceiving;
	
	private int _isRequesting;
	private readonly ManualResetEvent _receiveDone = new ManualResetEvent(false);
	private bool _receiveResult;

	public event Action<int> AssetReceived;

	public override ServiceId ServiceId => ServiceId.Asset;
	
	public AssetIndex Index { get; private set; }
	
	public int Received { get; private set; }
	public int AssetCount { get; private set; }
	
	public string AssetDirectory { get; set; } =  "assets";

	public override void Receive(NetCall call)
	{
		var sid = (AssetSid)call.Stream.ReadExactByte();

		switch (sid)
		{
		case AssetSid.SendIndex:
			HandleSendIndex(call);
			break;
		case AssetSid.SendAsset:
			HandleSendAsset(call);
			break;
		case AssetSid.RequestAsset:
			HandleRequestDenied(call);
			break;
		default:
			Logger.Error($"Unrecognised asset sid: {sid}.");
			break;
		}
	}
	
	private async Task<bool> RequestAssetAsync(int assetIdx)
	{
		if (Interlocked.CompareExchange(ref _isRequesting, 1, 0) != 0)
		{
			Logger.Error("Cannot sent request: An asset request already pending.");
			return false;
		}

		try
		{
			_receiveDone.Reset();

			await SendAsync((byte)AssetSid.RequestAsset, new SerialInt32(assetIdx));

			_receiveDone.WaitOne();

			return _receiveResult;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
		finally
		{
			Interlocked.Exchange(ref _isRequesting, 0);
		}
	}

	private async Task<bool> LoopRequestAssetAsync(int assetIdx, int maxAttempts = 3)
	{
		for (int i = 0; i < maxAttempts; i++)
		{
			if (await RequestAssetAsync(assetIdx))
			{
				return true;
			}
		}
		
		Logger.Error($"Exceeded max attempts of {maxAttempts} requesting asset {assetIdx}.");
		return false;
	}

	private async Task<bool> RequestAssetsAsync()
	{
		if (Index == null)
		{
			Logger.Error("Cannot request assets: No asset index available.");
			return false;
		}

		if (Interlocked.CompareExchange(ref _isReceiving, 1, 0) != 0)
		{
			Logger.Error("Cannot start receiving assets: Already receiving.");
			return false;
		}

		Received = 0;
		AssetCount = Index.Assets.Length;

		try
		{
			int successful = 0;

			for (int i = 0; i < Index.Assets.Length; i++)
			{
				if (await LoopRequestAssetAsync(i))
				{
					successful++;
				}

				Received++;
				AssetReceived?.Invoke(i);
			}

			Logger.Info($"Successfully received {successful}/{Index.Assets.Length} assets.");
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
		finally
		{
			Interlocked.Exchange(ref _isReceiving, 0);
		}
	}

	private void HandleSendIndex(NetCall call)
	{
		try
		{
			Directory.CreateDirectory(AssetDirectory);
			
			Index = AssetIndex.Deserialize(AssetDirectory, call.Stream);
			
			Logger.Info($"Received asset index with {Index.Assets.Length} assets.");

			Task.Run(RequestAssetsAsync);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
		}
	}
	
	private void HandleSendAsset(NetCall call)
	{
		try
		{
			var assetIdx = call.Stream.ReadNetInt32();

			if (assetIdx < 0 || assetIdx >= Index.Assets.Length)
			{
				Logger.Error($"Received invalid asset index {assetIdx}.");
				EndReceive(false);
				return;
			}
			
			var path = Path.Combine(AssetDirectory, Index.Assets[assetIdx]);

			using (var fs = File.Open(path, FileMode.Create, FileAccess.Write))
			{
				call.Stream.CopyTo(fs);
			}
			
			EndReceive(true);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			EndReceive(false);
		}
	}

	private void HandleRequestDenied(NetCall call)
	{
		try
		{
			var reason = call.Stream.ReadNetUtf8();
			
			Logger.Info($"Asset request denied: {reason}");
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
		}
		finally
		{
			EndReceive(false);
		}
	}

	private void EndReceive(bool result)
	{
		_receiveResult = result;
		_receiveDone.Set();
	}
}