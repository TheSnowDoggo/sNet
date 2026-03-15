using System.Collections.Concurrent;
using sNet.Server;

namespace sNet.CScriptPro;

public abstract class PartRoot
{
	protected readonly UidRegistry<Part> _registry = [];
	
	public PartRoot()
	{
		Root = new Part
		{
			Root = this,
		};
	}

	public readonly Part Root;
	
	public readonly ConcurrentQueue<(Uid Uid, Obj[] Args)> EventQueue = [];
	
	public readonly EventPack EventPack = new EventPack();

	public Obj GetPartByUid(Uid uid)
	{
		return _registry.TryGetValue(uid, out var part) ? part : Nil.Value;
	}
	
	public void QueueEvents(Stream stream, int clientIdx = -1)
	{
		try
		{
			var queue = EventPack.Deserialize(stream);

			while (queue.TryDequeue(out var info))
			{
				if (clientIdx != -1)
				{
					info.Args = [clientIdx, ..info.Args];
				}
				
				EventQueue.Enqueue(info);
			}
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
		}
	}
	
	public abstract void RunEvent(RemoteEvent remoteEvent, Obj[] args);
	
	public virtual void Update(double delta)
	{
		ProcessEvents();
		
		Event.Update.Fire([delta]);
	}

	public virtual void PartAdded(Part root)
	{
	}

	public virtual void PartRemoved(Part root)
	{
	}

	public virtual void PropertyUpdate(Part part, string name, Obj value)
	{
	}

	public abstract void FireAllClients(RemoteEvent remoteEvent, Obj[] args);

	public abstract void FireClient(RemoteEvent remoteEvent, int idx, Obj[] args);

	public abstract void FireServer(RemoteEvent remoteEvent, Obj[] args);

	private void ProcessEvents()
	{
		// Ensures events received before the initial part send can be processed
		if (Root.Children.Count == 0)
		{
			return;
		}
		
		var requeue = new Queue<(Uid, Obj[])>();
		
		while (EventQueue.TryDequeue(out var info))
		{
			if (!_registry.TryGetValue(info.Uid, out var part))
			{
				Logger.Error($"Failed to run event: No remove event found with uid {info.Uid}.");
				requeue.Enqueue(info);
				continue;
			}

			if (part is not RemoteEvent remoteEvent)
			{
				Logger.Error($"Failed to run event: No referenced part was {part.TypeId}, not a RemoteEvent.");
				continue;
			}
			
			RunEvent(remoteEvent, info.Args);
		}

		while (requeue.TryDequeue(out var info))
		{
			EventQueue.Enqueue(info);
		}
	}
}