namespace sNet;

public sealed class SerialInt32 : INetSerializable
{
	private readonly int _value;

	public SerialInt32(int value)
	{
		_value = value;
	}

	public int MaxSize => sizeof(int);

	public void Serialize(NetSerializer serial)
	{
		serial.WriteInt32(_value);
	}
}