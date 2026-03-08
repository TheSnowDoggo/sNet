namespace sNet;

public interface INetPackage : INetSerializable
{
	bool IsEmpty { get; }
}