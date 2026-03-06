namespace sNet;

public class NetSerializer : IDisposable
{
	protected readonly Stream _stream;
	
	public NetSerializer(Stream stream)
	{
		_stream = stream;
	}
	
	public int WrittenBytes { get; protected set; }

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
	
	public void WriteBoolean(bool value)
	{
		WrittenBytes += _stream.WriteBoolean(value);
	}
	
	public void WriteInt16(short value)
	{
		WrittenBytes += _stream.WriteNetInt16(value);
	}
	
	public void WriteUInt16(ushort value)
	{
		WrittenBytes += _stream.WriteNetUInt16(value);
	}
	
	public void WriteChar(char value)
	{
		WrittenBytes += _stream.WriteNetChar(value);
	}

	public void WriteInt32(int value)
	{
		WrittenBytes += _stream.WriteNetInt32(value);
	}

	public void WriteUInt32(uint value)
	{
		WrittenBytes += _stream.WriteNetUInt32(value);
	}
	
	public void WriteSingle(float value)
	{
		WrittenBytes += _stream.WriteNetSingle(value);
	}

	public void WriteInt64(long value)
	{
		WrittenBytes += _stream.WriteNetInt64(value);
	}

	public void WriteUInt64(ulong value)
	{
		WrittenBytes += _stream.WriteNetUInt64(value);
	}
	
	public void WriteDouble(double value)
	{
		WrittenBytes += _stream.WriteNetDouble(value);
	}

	public void WriteUtf8(string value)
	{
		WrittenBytes += _stream.WriteNetUtf8(value);
	}
	
	public void WriteUtf16(string value)
	{
		WrittenBytes += _stream.WriteNetUtf16(value);
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