using sNet.Service.Part;

namespace sNet.CScriptPro;

public sealed class ServerPartRoot : PartRoot
{
	private readonly UidRegistry<Part> _registry = [];

	public ServerPartService Service { get; set; }

	public AddNetPack AddNetPack { get; } = new AddNetPack();
	public RemoveNetPack RemoveNetPack { get; } = new RemoveNetPack();
	public UpdateNetPack UpdateNetPack { get; } = new UpdateNetPack();

	public override void Update(double delta)
	{
		if (Service == null)
		{
			return;
		}

		Service.FireBroadcast(PartSid.Remove, RemoveNetPack, ServerPartService.MaxRemoveSize);
		Service.FireBroadcast(PartSid.Add, AddNetPack, ServerPartService.MaxAddSize);
		Service.FireBroadcast(PartSid.Update, UpdateNetPack, ServerPartService.MaxUpdateSize);
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

			part.Uid = _registry.AddNew(part);
			part.Root = this;
		}
		
		if (Service != null)
		{
			AddNetPack.Enqueue(root);
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
				RemoveNetPack.Enqueue(part.Uid);
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
			UpdateNetPack.Enqueue(part.Uid, name, value);
		}
	}
}