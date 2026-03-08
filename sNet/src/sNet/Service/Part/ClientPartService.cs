using sNet.CScriptPro;

namespace sNet.Service.Part;

public class ClientPartService : ClientService
{
	public ClientPartRoot Root { get; set; }
	
	public override ServiceId ServiceId => ServiceId.Part;

	public override void Receive(NetCall call)
	{
		var sid = (PartSid)call.Stream.ReadExactByte();

		switch (sid)
		{
		case PartSid.Add:
			HandleAdd(call);
			break;
		case PartSid.Remove:
			HandleRemove(call);
			break;
		case PartSid.Update:
			HandleUpdate(call);
			break;
		case PartSid.FireClient:
			HandleFireClient(call);
			break;
		default:
			Logger.Error($"Unrecognised part sid: {sid}");
			break;
		}
	}
	
	public async Task<bool> SendEvent(EventPack events)
	{
		return await SendPackAsync((byte)PartSid.FireServer, events);
	}
	
	private void HandleAdd(NetCall call)
	{
		try
		{
			if (Root == null)
			{
				Logger.Error("Received add package but not root has been assigned.");
				return;
			}

			var queue = AddPack.Deserialize(call.Stream);
			
			Logger.Info($"Received add package with {queue.Count} queued.");

			while (queue.TryDequeue(out var addEvent))
			{
				Root.AddQueue.Enqueue(addEvent);
			}
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
		}
	}

	private void HandleRemove(NetCall call)
	{
		try
		{
			if (Root == null)
			{
				Logger.Error("Received remove package but not root has been assigned.");
				return;
			}

			var queue = RemovePack.Deserialize(call.Stream);

			while (queue.TryDequeue(out var removeEvent))
			{
				Root.RemoveQueue.Enqueue(removeEvent);
			}
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
		}
	}

	private void HandleUpdate(NetCall call)
	{
		try
		{
			if (Root == null)
			{
				Logger.Error("Received update package but not root has been assigned.");
				return;
			}

			var queue = UpdatePack.Deserialize(call.Stream);

			while (queue.TryDequeue(out var info))
			{
				Root.UpdateQueue.Enqueue(info);
			}
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
		}
	}

	private void HandleFireClient(NetCall call)
	{
		if (Root == null)
		{
			Logger.Error("Failed to handle event: No root assigned.");
			return;
		}
        
		Root.QueueEvents(call.Stream);
	}
}