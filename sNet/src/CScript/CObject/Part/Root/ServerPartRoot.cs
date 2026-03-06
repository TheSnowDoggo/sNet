namespace sNet.CScriptPro;

public sealed class ServerPartRoot : PartRoot
{
	private readonly UidRegistry<Part> _registry = [];
	
	public UpdateNetPack UpdateNetPack { get; } = new UpdateNetPack();
	
	public override void PartAdded(Part root)
	{
		foreach (var part in root.Descendants())
		{
			if (part.Root != null)
			{
				throw new InvalidOperationException($"Cannot add part {part}: Root was not null.");
			}
			
			if (part.Uid != Uid.Null)
			{
				throw new InvalidOperationException($"Cannot add part {part}: Uid (\'{part.Uid}\') was not null.");
			}

			part.Uid = _registry.AddNew(part);
			part.Root = this;
		}
	}

	public override void PartRemoved(Part root)
	{
		foreach (var part in root.Descendants())
		{
			if (part.Root != this)
			{
				throw new InvalidOperationException($"Cannot remove part {part}: Root was either null or not part of this root.");
			}
			
			if (part.Uid == Uid.Null)
			{
				throw new InvalidOperationException($"Failed to remove part {part}: Uid was null.");
			}

			if (!_registry.Remove(part.Uid))
			{
				throw new InvalidOperationException($"Failed to remove part {part}: Uid (\'{part.Uid}\') was not found.");
			}
			
			part.Uid = Uid.Null;
			part.Root = null;
		}
	}

	public override void PropertyUpdate(Part part, string name, CObj value)
	{
		if (!_registry.ContainsKey(part.Uid))
		{
			throw new InvalidOperationException($"Cannot queue property update for {part}: ServerUid was not found.");
		}
		
		UpdateNetPack.Enqueue(part.Uid, name, value);
	}
}