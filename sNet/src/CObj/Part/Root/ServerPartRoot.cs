using sNet.Service.Part;

namespace sNet.CScriptPro;

public sealed class ServerPartRoot : PartRoot
{
	public ServerPartService Service { get; set; }

	public readonly AddPack AddPack = new AddPack();
	public readonly RemovePack RemovePack = new RemovePack();
	public readonly UpdatePack UpdatePack = new UpdatePack();

	public readonly Dictionary<int, EventPack> DirectEvents = [];
	
	public override void Update(double delta)
	{
		base.Update(delta);
		
		if (Service == null || Service.Server.Clients.Count == 0)
		{
			return;
		}
		
		Task.WaitAll(Service.BroadcastRemove(RemovePack),
			Service.BroadcastAdd(AddPack),
			Service.BroadcastUpdate(UpdatePack),
			Service.BroadcastEvent(EventPack));
		
		ProcessDirectEvents();
	}

	public override void RunEvent(RemoteEvent remoteEvent, Obj[] args)
	{
		remoteEvent.Server.Fire(args);
	}

	public override void FireAllClients(RemoteEvent remoteEvent, Obj[] args)
	{
		EventPack.Enqueue(remoteEvent.Uid, args);
	}

	public override void FireClient(RemoteEvent remoteEvent, int idx, Obj[] args)
	{
		if (Service == null)
		{
			Logger.Error("Failed to queue client fire: No service was assigned");
			return;
		}

		if (!Service.Server.Clients.HasClient(idx))
		{
			Logger.Error($"Unknown client with idx {idx}.");
			return;
		}
		
		DirectEvents.GetValueOrCreate(idx).Enqueue(remoteEvent.Uid, args);
	}

	public override void FireServer(RemoteEvent remoteEvent, Obj[] args)
	{
		Logger.Error("Cannot invoke client event from server.");
	}

	public override void PartAdded(Part root)
	{
		foreach (var part in root.Descendants())
		{
			if (part.Root != null)
			{
				Logger.Error($"Cannot add part {part}: Root was not null.");
				continue;
			}
			
			if (part.Uid != Uid.Null)
			{
				Logger.Error($"Cannot add part {part}: Uid (\'{part.Uid}\') was not null.");
				continue;
			}

			part.Root = this;
			part.Uid = _registry.AddNew(part);
		}
		
		if (Service != null)
		{
			AddPack.Enqueue(root);
		}
	}

	public override void PartRemoved(Part root)
	{
		foreach (var part in root.Descendants())
		{
			if (part.Root != this)
			{
				Logger.Error($"Cannot remove part {part}: Root was either null or not part of this root.");
				continue;
			}
			
			if (part.Uid == Uid.Null)
			{
				Logger.Error($"Failed to remove part {part}: Uid was null.");
				continue;
			}

			if (!_registry.Remove(part.Uid))
			{
				Logger.Error($"Failed to remove part {part}: Uid (\'{part.Uid}\') was not found.");
				continue;
			}
			
			if (Service != null)
			{
				RemovePack.Enqueue(part.Uid);
			}
			
			part.Uid = Uid.Null;
			part.Root = null;
		}
	}

	public override void PropertyUpdate(Part part, string name, Obj value)
	{
		if (!_registry.ContainsKey(part.Uid))
		{
			Logger.Error($"Cannot queue property update for {part}: ServerUid was not found.");
			return;
		}

		if (Service != null)
		{
			UpdatePack.Enqueue(part.Uid, name, value);
		}
	}

	private void ProcessDirectEvents()
	{
		if (DirectEvents.Count == 0)
		{
			return;
		}

		var directTasks = new List<Task>();
		
		foreach ((int idx, EventPack pack) in DirectEvents)
		{
			directTasks.Add(Service.SendEvent(idx, pack));
		}

		DirectEvents.Clear();
		
		Task.WaitAll(directTasks.ToArray());
	}
}