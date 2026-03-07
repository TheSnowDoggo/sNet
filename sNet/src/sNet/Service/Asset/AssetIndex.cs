namespace sNet.Service.Asset;

public sealed class AssetIndex : INetPackage
{
	public const int MaxIndexPathSize = 256;
	
	public AssetIndex(string directory, string[] assets)
	{
		Directory = directory;
		Assets = assets;
	}
	
	public string Directory { get; }
	public string[] Assets { get; }

	public bool IsEmpty => false;

	public int MaxSize => MaxIndexPathSize * Assets.Length;

	public static AssetIndex Discover(string directory)
	{
		var assets = new List<string>();
		
		foreach (var file in FileUtils.EnumerateFilesRecursive(directory))
		{
			assets.Add(Path.GetRelativePath(directory, file));
		}
		
		return new AssetIndex(directory, assets.ToArray());
	}

	public static bool TryDiscover(string directory, out AssetIndex index)
	{
		try
		{
			index = Discover(directory);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			index = null;
			return false;
		}
	}

	public static AssetIndex Deserialize(string directory, Stream stream)
	{
		int count = stream.ReadNetInt32();

		if (count < 0)
		{
			throw new InvalidDataException($"Asset count (\'{count}\') was negative.");
		}
		
		var assets = new string[count];
		
		for (int i = 0; i < count; i++)
		{
			assets[i] = stream.ReadNetUtf8();
		}
		
		return new AssetIndex(directory, assets);
	}
	
	public void Serialize(NetSerializer serial)
	{
		serial.WriteInt32(Assets.Length);

		foreach (var asset in Assets)
		{
			serial.WriteUtf8(asset);
		}
	}
}