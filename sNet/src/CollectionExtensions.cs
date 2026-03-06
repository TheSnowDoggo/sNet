namespace sNet;

public static class CollectionExtensions
{
	public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		where TKey : notnull
		where TValue : new()
	{
		return dictionary.TryGetValue(key, out TValue value) ? value : dictionary[key] = new TValue();
	}
}