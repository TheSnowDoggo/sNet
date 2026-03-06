namespace sNet.CScriptPro;

public sealed class RemoveNetPack : INetSerializable
{
	private HashSet<Uid> _removes = [];

	public bool Enqueue(Uid part)
	{
		return _removes.Add(part);	
	}

	public static Queue<Uid> Deserialize(Stream stream)
	{
		int count = stream.ReadNetInt32();
		
		var queue = new Queue<Uid>(count);

		for (int i = 0; i < count; i++)
		{
			queue.Enqueue(stream.ReadNetInt64());
		}
		
		return queue;
	}
	
	public void Serialize(NetSerializer serial)
	{
		var removes = Interlocked.Exchange(ref _removes, []);
		
		serial.WriteInt32(removes.Count);

		foreach (var uid in removes)
		{
			serial.WriteInt64(uid);
		}
	}
}