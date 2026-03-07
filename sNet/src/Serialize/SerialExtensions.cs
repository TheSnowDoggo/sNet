using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;
using sNet.CScriptPro;

namespace sNet;

public static class SerialExtensions
{
	public static long Remaining(this Stream stream)
	{
		return stream.Length - stream.Position;
	}
	
	public static int WriteNetByte(this Stream stream, byte value)
	{
		stream.WriteByte(value);
		return sizeof(byte);
	}
	
	public static int WriteBoolean(this Stream stream, bool value)
	{
		stream.WriteByte(Unsafe.BitCast<bool, byte>(value));
		return sizeof(bool);
	}
	
	public static int WriteNetInt16(this Stream stream, short value)
	{
		Span<byte> span = stackalloc byte[sizeof(short)];
		
		int network = IPAddress.HostToNetworkOrder(value);
		
		Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), network);
		
		stream.Write(span);
		
		return sizeof(short);
	}
	
	public static int WriteNetUInt16(this Stream stream, ushort value)
	{
		stream.WriteNetInt16(Unsafe.BitCast<ushort, short>(value));
		return sizeof(ushort);
	}
	
	public static int WriteNetChar(this Stream stream, char value)
	{
		stream.WriteNetInt16(Unsafe.BitCast<char, short>(value));
		return sizeof(char);
	}
	
	public static int WriteNetInt32(this Stream stream, int value)
	{
		Span<byte> span = stackalloc byte[sizeof(int)];
		
		int network = IPAddress.HostToNetworkOrder(value);
		
		Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), network);
		
		stream.Write(span);
		return sizeof(int);
	}
	
	public static int WriteNetUInt32(this Stream stream, uint value)
	{
		stream.WriteNetInt32(Unsafe.BitCast<uint, int>(value));
		return sizeof(uint);
	}

	public static int WriteNetSingle(this Stream stream, float value)
	{
		stream.WriteNetInt32(Unsafe.BitCast<float, int>(value));
		return sizeof(float);
	}
	
	public static int WriteNetInt64(this Stream stream, long value)
	{
		Span<byte> span = stackalloc byte[sizeof(long)];
		
		long network = IPAddress.HostToNetworkOrder(value);
		
		Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), network);
		
		stream.Write(span);
		return sizeof(long);
	}
	
	public static int WriteNetUInt64(this Stream stream, ulong value)
	{
		stream.WriteNetInt64(Unsafe.BitCast<ulong, long>(value));
		return sizeof(ulong);
	}
	
	public static int WriteNetDouble(this Stream stream, double value)
	{
		stream.WriteNetInt64(Unsafe.BitCast<double, long>(value));
		return sizeof(double);
	}
	
	public static int WriteNetUtf8(this Stream stream, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			stream.WriteNetInt32(0);
			stream.WriteNetInt32(0);
			
			return 2 * sizeof(int);
		}

		using var buffer = RentBuffer.Share(value.Length * 3);

		Utf8.FromUtf16(value.AsSpan(), buffer, out _, out int written);

		stream.WriteNetInt32(written);
		stream.WriteNetInt32(value.Length);
		stream.Write(buffer.Data, 0, written);

		return 2 * sizeof(int) + written;
	}

	public static int WriteNetUtf16(this Stream stream, string value)
	{
		if (stream.Position + value.Length * sizeof(char) > stream.Length)
		{
			throw new EndOfStreamException("String will exceed stream bounds.");
		}

		stream.WriteNetInt32(value.Length);
		
		Span<byte> span = stackalloc byte[sizeof(char)];
		
		for (int i = 0; i < value.Length; i++)
		{
			short network = IPAddress.HostToNetworkOrder(Unsafe.BitCast<char, short>(value[i]));
			
			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), network);
			
			stream.Write(span);
		}

		return sizeof(int) + value.Length * sizeof(char);
	}
	
	public static int WriteCObj(this Stream stream, Obj obj)
	{
		stream.WriteByte((byte)obj.TypeId);

		return sizeof(byte) + obj.TypeId switch
		{
			TypeId.Nil => 0,
			TypeId.Number => stream.WriteNetDouble((Number)obj),
			TypeId.String => stream.WriteNetUtf8((StrObj)obj),
			TypeId.Bool => stream.WriteBoolean((Bool)obj),
			TypeId.Array => stream.WriteArray((ArrayObj)obj),
			TypeId.Table => stream.WriteTable((UserTable)obj),
			TypeId.Vec2 => stream.WriteVec2((Vec2Obj)obj),
			TypeId.Part => stream.WritePart((Part)obj),
			TypeId.Uid => stream.WriteUid((UidObj)obj),
			_ => throw new InvalidOperationException($"Cannot serialize type {obj.TypeId}."),
		};
	}

	public static int WriteArray(this Stream stream, ArrayObj arrayObj)
	{
		stream.WriteNetInt32(arrayObj.Count);

		int written = sizeof(int);

		foreach (var item in arrayObj)
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

	public static int WriteVec2(this Stream stream, Vector2 vector2)
	{
		return stream.WriteNetDouble(vector2.X) + stream.WriteNetDouble(vector2.Y);
	}

	public static int WritePart(this Stream stream, Part part)
	{
		return part.Serialize(stream);
	}

	public static int WriteUid(this Stream stream, Uid uid)
	{
		return stream.WriteNetInt64(uid);
	}

	public static byte ReadExactByte(this Stream stream)
	{
		int value = stream.ReadByte();

		if (value == -1)
		{
			throw new EndOfStreamException("Tried to read byte but stream was empty.");
		}
		
		return (byte)value;
	}

	public static bool ReadBoolean(this Stream stream)
	{
		return Unsafe.BitCast<byte, bool>(stream.ReadExactByte());
	}

	public static short ReadNetInt16(this Stream stream)
	{
		Span<byte> span = stackalloc byte[sizeof(short)];
		
		stream.ReadExactly(span);
		
		short network = Unsafe.ReadUnaligned<short>(ref MemoryMarshal.GetReference(span));
		
		return IPAddress.NetworkToHostOrder(network);
	}

	public static ushort ReadNetUInt16(this Stream stream)
	{
		return Unsafe.BitCast<short, ushort>(stream.ReadNetInt16());
	}

	public static char ReadNetChar(this Stream stream)
	{
		return Unsafe.BitCast<short, char>(stream.ReadNetInt16());
	}
	
	public static int ReadNetInt32(this Stream stream)
	{
		Span<byte> span = stackalloc byte[sizeof(int)];
		
		stream.ReadExactly(span);

		int network = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(span));
		
		return IPAddress.NetworkToHostOrder(network);
	}
	
	public static uint ReadNetUInt32(this Stream stream)
	{
		return Unsafe.BitCast<int, uint>(stream.ReadNetInt32());
	}

	public static float ReadNetSingle(this Stream stream)
	{
		return Unsafe.BitCast<int, float>(stream.ReadNetInt32());
	}
	
	public static long ReadNetInt64(this Stream stream)
	{
		Span<byte> span = stackalloc byte[sizeof(long)];
		
		stream.ReadExactly(span);
		
		long network = Unsafe.ReadUnaligned<long>(ref MemoryMarshal.GetReference(span));
		
		return IPAddress.NetworkToHostOrder(network);
	}

	public static ulong ReadNetUInt64(this Stream stream)
	{
		return Unsafe.BitCast<long, ulong>(stream.ReadNetInt64());
	}

	public static double ReadNetDouble(this Stream stream)
	{
		return Unsafe.BitCast<long, double>(stream.ReadNetInt64());
	}
	
	public static string ReadNetUtf8(this Stream stream)
	{
		int utf8Bytes = stream.ReadNetInt32();

		if (utf8Bytes < 0)
		{
			throw new InvalidDataException($"Utf8 length (\'{utf8Bytes}\') was invalid.");
		}
		
		int utf16Bytes = stream.ReadNetInt32();
		
		if (utf16Bytes < 0)
		{
			throw new InvalidDataException($"Utf16 length (\'{utf8Bytes}\') was invalid.");
		}

		if (utf8Bytes == 0)
		{
			return string.Empty;
		}
		
		using var buffer = RentBuffer.Share(utf8Bytes);

		stream.ReadExactly(buffer);

		return string.Create(utf16Bytes, (ReadOnlySpan<byte>)buffer, SpanAction);

		void SpanAction(Span<char> span, ReadOnlySpan<byte> state)
		{
			Utf8.ToUtf16(state, span, out _, out _, replaceInvalidSequences: false);
		}
	}

	public static string ReadNetUtf16(this Stream stream)
	{
		int length = stream.ReadNetInt32();

		if (length < 0)
		{
			throw new InvalidDataException($"String length (\'{length}\') was negative.");
		}

		if (stream.Position + length * sizeof(char) > stream.Length)
		{
			throw new EndOfStreamException($"String length (\'{length}\') exceeds remaining length of stream.");
		}

		return string.Create(length, stream, SpanAction);

		void SpanAction(Span<char> span, Stream state)
		{
			for (int i = 0; i < span.Length; i++)
			{
				span[i] = state.ReadNetChar();
			}
		}
	}
	
	public static Obj ReadCObj(this Stream stream)
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
			TypeId.Uid => stream.ReadNetInt64(),
			_ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(TypeId)),
		};
	}
	
	public static ArrayObj ReadArray(this Stream stream)
	{
		int count = stream.ReadNetInt32();

		if (count < 0)
		{
			throw new InvalidDataException($"Array count (\'{count}\') was negative.");
		}

		var list = new List<Obj>(count);

		for (int i = 0; i < count; i++)
		{
			list.Add(stream.ReadCObj());
		}

		return new ArrayObj(list);
	}

	public static UserTable ReadTable(this Stream stream)
	{
		int count = stream.ReadNetInt32();

		if (count < 0)
		{
			throw new InvalidDataException($"Table count (\'{count}\') was negative.");
		}

		var data = new Dictionary<Obj, Obj>();

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

	public static Vec2Obj ReadVec2(this Stream stream)
	{
		var x = stream.ReadNetDouble();
		var y = stream.ReadNetDouble();
		return new Vec2Obj(x, y);
	}

	public static Part ReadPart(this Stream stream)
	{
		return Part.DeserializeNew(stream);
	}
}