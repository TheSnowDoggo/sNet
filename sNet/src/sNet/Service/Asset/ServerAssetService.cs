using sNet.Server;

namespace sNet.Service.Asset;

public sealed class ServerAssetService : ServerService
{
	public override ServiceId ServiceId => ServiceId.Asset;
	
	public AssetIndex Index { get; set; }

	public override void Receive(ServerNetCall call)
	{
		var sid = (AssetSid)call.Stream.ReadExactByte();

		switch (sid)
		{
		case AssetSid.RequestAsset:
			HandleRequestAsset(call);
			break;
		default:
			Logger.Error($"Unrecognised asset sid: {sid}.");
			break;
		}
	}
	
	public override void ClientJoined(RemoteClient client)
	{
		if (Index == null)
		{
			Logger.Error($"Cannot send index to {client}: No index assigned.");
			return;
		}
		
		Task.Run(() => SendIndexAsync(client.Idx, Index));
		
		Logger.Info($"Sending index to {client}.");
	}

	private async Task<bool> SendIndexAsync(int idx, AssetIndex index)
	{
		return await SendPackAsync(idx, (byte)AssetSid.SendIndex, index);
	}

	private async Task<bool> SendAssetAsync(int idx, int assetIdx)
	{
		try
		{
			var name = Index.Assets[assetIdx];
			var path = Path.Combine(Index.Directory, name);

			await using var fs = File.OpenRead(path);

			if (fs.Length > int.MaxValue)
			{
				Logger.Error($"Cannot sent asset file {name}, file is too large.");
				FireDenial(idx, "FILE_TOO_LARGE");
				return false;
			}

			int length = sizeof(int) + sizeof(byte) * 2 + sizeof(int) + (int)fs.Length;

			using var buffer = RentBuffer.Share(length);
			using var serial = buffer.OpenSerial();
			
			serial.Begin();
			
			serial.WriteByte((byte)ServiceId);
			serial.WriteByte((byte)AssetSid.SendAsset);
			serial.WriteInt32(assetIdx);
			await serial.WriteStreamAsync(fs);
			
			serial.End();
			buffer.Trim(serial.WrittenBytes);

			return await Server.SendAsync(idx, buffer);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			FireDenial(idx, "FAILED");
			return false;
		}
	}

	private void HandleRequestAsset(ServerNetCall call)
	{
		if (Index == null)
		{
			Logger.Error($"Cannot response to asset request from {call.Client}: No index assigned.");
			FireDenial(call.Client.Idx, "NO_INDEX_ASSIGNED");
			return;
		}
		
		try
		{
			int assetIdx = call.Stream.ReadNetInt32();

			if (assetIdx < 0 || assetIdx >= Index.Assets.Length)
			{
				Logger.Error($"Client {call.Client} requested non-existant asset with idx {assetIdx}.");
				FireDenial(call.Client.Idx, "INVALID_ASSET_IDX");
				return;
			}
			
			Task.Run(() => SendAssetAsync(call.Client.Idx, assetIdx));
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			FireDenial(call.Client.Idx, "FAILED");
		}
	}

	private void FireDenial(int idx, string reason)
	{
		Task.Run(() => SendDenialAsync(idx, reason));
	}
	
	private async Task<bool> SendDenialAsync(int idx, string reason)
	{
		return await SendAsync(idx, (byte)AssetSid.RequestDenied, new SerialString(reason));
	}
}