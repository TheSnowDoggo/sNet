namespace sNet.CScriptPro;

public sealed class EventPack : INetPackage
{
	public const int MaxEventSize = 1024;

	private Queue<(Uid Uid, Obj[] Args)> _queue = [];

	public void Enqueue(Uid uid, Obj[] args)
	{
		_queue.Enqueue((uid, args));
	}
	
	public bool IsEmpty => _queue.Count == 0;

	public int MaxSize => MaxEventSize;

	public static Queue<(Uid Uid, Obj[] Args)> Deserialize(Stream stream)
	{
		int count = stream.ReadNetInt32();

		if (count < 0)
		{
			throw new InvalidDataException($"Queue count (\'{count}\') was negative.");
		}

		var queue = new Queue<(Uid, Obj[])>();

		for (int i = 0; i < count; i++)
		{
			var uid = (Uid)stream.ReadNetInt64();
			var args = stream.ReadArgs();
			
			queue.Enqueue((uid, args));
		}

		return queue;
	}

	public void Serialize(NetSerializer serial)
	{
		var queue = Interlocked.Exchange(ref _queue, []);
		
		serial.WriteInt32(queue.Count);

		while (queue.TryDequeue(out var info))
		{
			serial.WriteInt64(info.Uid);
			serial.WriteArgs(info.Args);
		}
	}
}