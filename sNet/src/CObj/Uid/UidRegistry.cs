namespace sNet.CScriptPro;

public sealed class UidRegistry<T> : Dictionary<Uid, T>
{
	private readonly Random _random = new Random();

	public UidRegistry() { }
	
	public UidRegistry(int capacity) : base(capacity) { }
	
	public UidRegistry(IReadOnlyDictionary<Uid, T> dictionary) : base(dictionary) { }

	public Uid AddNew(T value, int maxAttempts = 256)
	{
		for (int i = 0; i < maxAttempts; i++)
		{
			var uid = (Uid)_random.NextInt64();

			if (uid == Uid.Null)
			{
				continue;
			}

			if (TryAdd(uid, value))
			{
				return uid;
			}
		}

		throw new InvalidOperationException($"Exceeded max attempts {maxAttempts} to generate Uid.");
	}
}