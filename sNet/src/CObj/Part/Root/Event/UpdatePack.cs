namespace sNet.CScriptPro;

public sealed class UpdatePack : INetPackage
{
	public const int MaxUpdateSize = 4096;
	
	private UidRegistry<Dictionary<string, Obj>> _data = [];
	
	public bool IsEmpty => _data.Count == 0;

	public int MaxSize => MaxUpdateSize * _data.Count;

	public void Enqueue(Uid uid, string name, Obj obj)
	{
		_data.GetValueOrCreate(uid)[name] = obj;
	}
	
	public static Queue<(Uid Uid, UpdateEvent[])> Deserialize(Stream stream)
	{
		int queueCount = stream.ReadNetInt32();

		if (queueCount < 0)
		{
			throw new InvalidDataException($"Queue count (\'{queueCount}\') was negative.");
		}

		var queue = new Queue<(Uid, UpdateEvent[])>();

		for (int i = 0; i < queueCount; i++)
		{
			var uid = (Uid)stream.ReadNetInt64();
			var updateCount = stream.ReadNetInt32();

			if (updateCount < 0)
			{
				throw new InvalidDataException($"Update count (\'{updateCount}\') was negative.");
			}
			
			var updates = new UpdateEvent[updateCount];

			for (int j = 0; j < updateCount; j++)
			{
				var name = stream.ReadNetUtf8();
				var value = stream.ReadObj();
				
				updates[j] = new UpdateEvent(name, value);
			}
			
			queue.Enqueue((uid, updates));
		}

		return queue;
	}

	public void Serialize(NetSerializer serial)
	{
		var data = Interlocked.Exchange(ref _data, []);
		
		serial.WriteInt32(data.Count);

		foreach (var (uid, updates) in data)
		{
			serial.WriteInt64(uid);
			serial.WriteInt32(updates.Count);

			foreach (var (name, value) in updates)
			{
				serial.WriteUtf8(name);
				serial.WriteObj(value);
			}
		}
	}
}