namespace sNet;

public sealed class SerialString : INetSerializable
{
	private readonly string _value;

	public SerialString(string value)
	{
		_value = value;
	}

	public int MaxSize => _value.Length * 3;

	public void Serialize(NetSerializer serial)
	{
		serial.WriteUtf8(_value);
	}
}