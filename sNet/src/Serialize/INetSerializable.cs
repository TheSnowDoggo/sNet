namespace sNet;

public interface INetSerializable
{
	public int MaxSize { get; }
	
	public void Serialize(NetSerializer serial);
}