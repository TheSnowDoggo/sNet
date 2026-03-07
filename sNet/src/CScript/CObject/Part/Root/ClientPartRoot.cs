using System.Collections.Concurrent;

namespace sNet.CScriptPro;

public sealed class ClientPartRoot : PartRoot
{
	private readonly UidRegistry<Part> _registry = [];

	public ConcurrentQueue<AddEvent> AddQueue { get; } = [];
	public ConcurrentQueue<Uid> RemoveQueue { get; } = [];
	public ConcurrentQueue<(Uid Uid, UpdateEvent[] Updates)> UpdateQueue { get; } = [];
	
	public override void Update(double delta)
	{
		base.Update(delta);
		
		ProcessRemoveQueue();
		ProcessAddQueue();
		ProcessUpdateQueue();
	}

	public void Clear()
	{
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
		while (RemoveQueue.TryDequeue(out var uid))
		{
			if (!_registry.Remove(uid, out var part))
			{
				Logger.Error($"Failed to remove part: Uid {uid} not found.");
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
	}

	private void ProcessUpdateQueue()
	{
		while (UpdateQueue.TryDequeue(out var info))
		{
			(Uid uid, UpdateEvent[] updates) = info;

			if (!_registry.TryGetValue(uid, out var part))
			{
				Logger.Error($"Failed to update part: No part found with uid: {uid}.");
				continue;
			}
			
			foreach (var updateEvent in updates)
			{
				part[updateEvent.Name] = updateEvent.Value;
			}
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
			}
		}
	}
	
	private void Unregister(Part root)
	{
		foreach (var part in root.Descendants())
		{
			if (_registry.Remove(part.Uid))
			{
				Logger.Error($"Failed to unregister part: Uid {part.Uid} not found.");
			}
		}
	}
}