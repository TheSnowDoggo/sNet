using System.Collections.Concurrent;
using sNet.Service.Part;

namespace sNet.CScriptPro;

public sealed class ClientPartRoot : PartRoot
{
	public ClientPartService Service { get; set; }

	public readonly ConcurrentQueue<AddEvent> AddQueue = [];
	public readonly ConcurrentQueue<Uid> RemoveQueue = [];
	public readonly ConcurrentQueue<(Uid Uid, UpdateEvent[] Updates)> UpdateQueue = [];
	
	public override void Update(double delta)
	{
		base.Update(delta);
		
		ProcessRemoveQueue();
		ProcessAddQueue();
		ProcessUpdateQueue();

		if (Service == null)
		{
			return;
		}

		Service.SendEvent(EventPack).Wait();
	}

	public override void RunEvent(RemoteEvent remoteEvent, Obj[] args)
	{
		remoteEvent.Client.Fire(args);
	}

	public override void FireServer(RemoteEvent remoteEvent, Obj[] args)
	{
		EventPack.Enqueue(remoteEvent.Uid, args);
	}

	public override void FireAllClients(RemoteEvent remoteEvent, Obj[] args)
	{
		Logger.Error("Cannot invoke server event from client.");
	}

	public override void FireClient(RemoteEvent remoteEvent, int idx, Obj[] args)
	{
		Logger.Error("Cannot invoke server event from client.");
	}

	public void Clear()
	{
		Root.ClearChildren();
		
		_registry.Clear();
		
		AddQueue.Clear();
		RemoveQueue.Clear();
		UpdateQueue.Clear();
	}

	private void ProcessAddQueue()
	{
		while (AddQueue.TryDequeue(out var addEvent))
		{
			(Uid parentUid, Part part) = addEvent;

			Part parent;

			if (parentUid == Uid.Null)
			{
				parent = Root;
			}
			else if (!_registry.TryGetValue(parentUid, out parent))
			{
				Logger.Error($"Failed to add part: No parent found with uid {parentUid}.");
				continue;
			}

			if (!parent.AddChild(part))
			{
				Logger.Error("Failed to add part.");
				continue;
			}

			Register(part);
			
			Script.RunScripts(part, ScriptFlag.Client);
		}
	}

	private void ProcessRemoveQueue()
	{
		var requeue = new Queue<Uid>();
		
		while (RemoveQueue.TryDequeue(out var uid))
		{
			if (!_registry.Remove(uid, out var part))
			{
				Logger.Error($"Failed to remove part: Uid {uid} not found.");
				requeue.Enqueue(uid);
				continue;
			}

			if (part.Parent == null)
			{
				Logger.Error("Failed to remove part: Part had no parent.");
				continue;
			}

			if (!part.Parent.RemoveChild(part))
			{
				Logger.Error("Failed to remove part: Part not found in parent.");
				continue;
			}

			Unregister(part);
		}

		while (requeue.TryDequeue(out var uid))
		{
			RemoveQueue.Enqueue(uid);
		}
	}

	private void ProcessUpdateQueue()
	{
		var requeue = new Queue<(Uid, UpdateEvent[])>();
		
		while (UpdateQueue.TryDequeue(out var info))
		{
			if (!_registry.TryGetValue(info.Uid, out var part))
			{
				Logger.Error($"Failed to update part: No part found with uid: {info.Uid}.");
				requeue.Enqueue(info);
				continue;
			}
			
			foreach (var updateEvent in info.Updates)
			{
				part[updateEvent.Name] = updateEvent.Value;
			}
		}

		while (requeue.TryDequeue(out var info))
		{
			UpdateQueue.Enqueue(info);
		}
	}

	private void Register(Part root)
	{
		foreach (var part in root.Descendants())
		{
			if (part.Uid == Uid.Null)
			{
				Logger.Error($"Failed to register part {part.Name}: Uid was null.");
				continue;
			}
			
			if (!_registry.TryAdd(part.Uid, part))
			{
				Logger.Error($"Failed to register part {part.Name}: Part with uid {part.Uid} already exists.");
				continue;
			}

			part.Root = this;
		}
	}
	
	private void Unregister(Part root)
	{
		foreach (var part in root.Descendants())
		{
			if (!_registry.Remove(part.Uid))
			{
				Logger.Error($"Failed to unregister part: Uid {part.Uid} not found.");
				continue;
			}

			part.Root = null;
		}
	}
}