using System.ComponentModel;

namespace sNet.CScriptPro;

public static class CObjExtensions
{
	public static int WriteCObj(this Stream stream, CObj obj)
	{
		stream.WriteByte((byte)obj.TypeId);

		return sizeof(byte) + obj.TypeId switch
		{
			TypeId.Nil => 0,
			TypeId.Number => stream.WriteNetDouble((Number)obj),
			TypeId.String => stream.WriteNetUtf8((CStr)obj),
			TypeId.Bool => stream.WriteBoolean((Bool)obj),
			TypeId.Array => stream.WriteArray((UserArray)obj),
			TypeId.Table => stream.WriteTable((UserTable)obj),
			TypeId.Vec2 => stream.WriteVec2((CVec2)obj),
			TypeId.Part => stream.WritePart((Part)obj),
			TypeId.Uid => stream.WriteUid((CUid)obj),
			_ => throw new InvalidOperationException($"Cannot serialize type {obj.TypeId}."),
		};
	}

	public static int WriteArray(this Stream stream, UserArray array)
	{
		stream.WriteNetInt32(array.Count);

		int written = sizeof(int);

		foreach (var item in array)
		{
			written += stream.WriteCObj(item);
		}
		
		return written;
	}
	
	public static int WriteTable(this Stream stream, UserTable table)
	{
		stream.WriteNetInt32(table.Count);

		int written = sizeof(int);

		foreach (var item in table)
		{
			written += stream.WriteCObj(item.Key);
			written += stream.WriteCObj(item.Value);
		}

		return written;
	}

	public static int WriteVec2(this Stream stream, Vec2 vec2)
	{
		return stream.WriteNetDouble(vec2.X) + stream.WriteNetDouble(vec2.Y);
	}

	public static int WritePart(this Stream stream, Part part)
	{
		return part.Serialize(stream);
	}

	public static int WriteUid(this Stream stream, Uid uid)
	{
		return stream.WriteNetInt64(uid);
	}
	
	public static CObj ReadCObj(this Stream stream)
	{
		var type = (TypeId)stream.ReadByte();

		return type switch
		{
			TypeId.Nil => Nil.Value,
			TypeId.Number => stream.ReadNetDouble(),
			TypeId.String => stream.ReadNetUtf8(),
			TypeId.Bool => stream.ReadBoolean(),
			TypeId.Array => stream.ReadArray(),
			TypeId.Table => stream.ReadTable(),
			TypeId.Vec2 => stream.ReadVec2(),
			TypeId.Part => stream.ReadPart(),
			TypeId.Uid => stream.ReadUid(),
			_ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(TypeId)),
		};
	}
	
	public static UserArray ReadArray(this Stream stream)
	{
		int count = stream.ReadNetInt32();

		if (count < 0)
		{
			throw new InvalidDataException($"Array count (\'{count}\') was negative.");
		}

		var list = new List<CObj>(count);

		for (int i = 0; i < count; i++)
		{
			list.Add(stream.ReadCObj());
		}

		return new UserArray(list);
	}

	public static UserTable ReadTable(this Stream stream)
	{
		int count = stream.ReadNetInt32();

		if (count < 0)
		{
			throw new InvalidDataException($"Table count (\'{count}\') was negative.");
		}

		var data = new Dictionary<CObj, CObj>();

		for (int i = 0; i < count; i++)
		{
			var key = stream.ReadCObj();
			var value = stream.ReadCObj();

			if (!data.TryAdd(key, value))
			{
				throw new InvalidDataException($"Table contained duplicate key named (\'{key}\').");
			}
		}
		
		return new UserTable(data);
	}

	public static CVec2 ReadVec2(this Stream stream)
	{
		var x = stream.ReadNetDouble();
		var y = stream.ReadNetDouble();
		return new CVec2(x, y);
	}

	public static Part ReadPart(this Stream stream)
	{
		return Part.DeserializeNew(stream);
	}

	public static CUid ReadUid(this Stream stream)
	{
		return (Uid)stream.ReadNetInt64();
	}
}