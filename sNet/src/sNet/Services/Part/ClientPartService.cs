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
		default:
			Logger.Error($"Unrecognised part sid: {sid}");
			break;
		}
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

			var queue = AddNetPack.Deserialize(call.Stream);

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

			var queue = RemoveNetPack.Deserialize(call.Stream);

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

			var queue = UpdateNetPack.Deserialize(call.Stream);

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
}