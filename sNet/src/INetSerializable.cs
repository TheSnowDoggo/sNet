namespace sNet;

public interface INetSerializable
{
	bool IsEmpty { get; }
	
	public void Serialize(NetSerializer serial);
}