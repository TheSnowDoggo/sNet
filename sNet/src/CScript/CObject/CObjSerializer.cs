namespace sNet.CScriptPro;

public sealed class CObjSerializer : NetSerializer
{
	public CObjSerializer(Stream stream)
		: base(stream) { }

	public void WriteCObj(CObj obj)
	{
		WrittenBytes += _stream.WriteCObj(obj);
	}

	public void WriteArray(UserArray value)
	{
		WrittenBytes += _stream.WriteArray(value);
	}

	public void WriteTable(UserTable value)
	{
		WrittenBytes += _stream.WriteTable(value);
	}

	public void WriteVec2(Vec2 value)
	{
		WrittenBytes += _stream.WriteVec2(value);
	}

	public void WritePart(Part value)
	{
		WrittenBytes += _stream.WritePart(value);
	}

	public void WriteUid(Uid value)
	{
		WrittenBytes += _stream.WriteUid(value);
	}
}