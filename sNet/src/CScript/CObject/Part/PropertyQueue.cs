namespace sNet.CScriptPro;

public sealed class PropertyQueue
{
	private UidRegistry<Dictionary<string, CObj>> _data = [];
	
	public PropertyQueue() { }
	public PropertyQueue(int capacity)
	{
		_data = new UidRegistry<Dictionary<string, CObj>>(capacity);
	}

	public UidRegistry<Dictionary<string, CObj>> Data => _data;

	public void Enqueue(Uid uid, string name, CObj obj)
	{
		if (!Data.TryGetValue(uid, out var updates))
		{
			Data[uid] = updates = [];
		}
		
		updates[name] = obj;
	}

	public static PropertyQueue Deserialize(Stream stream)
	{
		int queueCount = stream.ReadNetInt32();

		if (queueCount < 0)
		{
			throw new InvalidDataException($"Queue count (\'{queueCount}\') was negative.");
		}
		
		var queue = new PropertyQueue(queueCount);

		for (int i = 0; i < queueCount; i++)
		{
			var uid = stream.ReadUid();
			var updateCount = stream.ReadNetInt32();

			if (updateCount < 0)
			{
				throw new InvalidDataException($"Update count (\'{updateCount}\') was negative.");
			}

			var updates = new Dictionary<string, CObj>(updateCount);

			if (!queue._data.TryAdd(uid, updates))
			{
				throw new InvalidDataException($"Property queue contained duplicate Uid {uid}.");
			}

			for (int j = 0; j < updateCount; j++)
			{
				var name = stream.ReadNetUtf8();
				var value = stream.ReadCObj();

				if (!updates.TryAdd(name, value))
				{
					throw new InvalidDataException($"Update queue contained duplicate property name {name}.");
				}
			}
		}

		return queue;
	}

	public void Serialize(CObjSerializer serial)
	{
		var data = Interlocked.Exchange(ref _data, []);
		
		serial.WriteInt32(data.Count);

		foreach (var (uid, updates) in data)
		{
			serial.WriteUid(uid);
			serial.WriteInt32(updates.Count);

			foreach (var (name, value) in updates)
			{
				serial.WriteUtf8(name);
				serial.WriteCObj(value);
			}
		}
	}
}