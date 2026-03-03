namespace sNet;

public sealed class NetSerializer : IDisposable
{
	private readonly Stream _stream;
	
	public NetSerializer(Stream stream)
	{
		_stream = stream;
	}
	
	public int WrittenBytes { get; private set; }

	/// <summary>
	/// Makes room for the byte count header
	/// </summary>
	public void Begin()
	{
		_stream.Seek(sizeof(int), SeekOrigin.Begin);
	}

	public void WriteByte(byte value)
	{
		_stream.WriteByte(value);
		WrittenBytes += sizeof(byte);
	}

	public void WriteInt32(int value)
	{
		_stream.WriteNetInt32(value);
		WrittenBytes += sizeof(int);
	}

	public void WriteUInt32(uint value)
	{
		_stream.WriteNetUInt32(value);
		WrittenBytes += sizeof(uint);
	}

	public void WriteUtf8(string value)
	{
		WrittenBytes += _stream.WriteNetUtf8(value);
	}

	/// <summary>
	/// Writes the byte count header at the start
	/// </summary>
	public void End()
	{
		_stream.Seek(0, SeekOrigin.Begin);
		WriteInt32(WrittenBytes);
	}

	public void Dispose()
	{
		_stream.Dispose();
	}
}