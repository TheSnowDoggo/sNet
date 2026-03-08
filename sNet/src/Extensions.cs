namespace sNet;

public static class Extensions
{
	public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		where TKey : notnull
		where TValue : new()
	{
		return dictionary.TryGetValue(key, out TValue value) ? value : dictionary[key] = new TValue();
	}

	public static TEnum ParseOrDefault<TEnum>(string s, bool ignoreCase, TEnum defaultValue = default)
		where TEnum : struct
	{
		return Enum.TryParse(s, ignoreCase, out TEnum value) ? value : defaultValue;
	}
	
	public static TEnum ParseOrDefault<TEnum>(string s, TEnum defaultValue = default)
		where TEnum : struct
	{
		return Enum.TryParse(s, out TEnum value) ? value : defaultValue;
	}

	public static double NextDouble(this Random random, double max)
	{
		return double.Lerp(0, max, random.NextDouble());
	}
	
	public static double NextDouble(this Random random, double min, double max)
	{
		return double.Lerp(min, max, random.NextDouble());
	}
}