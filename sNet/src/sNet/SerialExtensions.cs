using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;
using CScriptPro;

namespace sNet;

public static class SerialExtensions
{
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

	public static int WriteCObject(this Stream stream, CObj obj)
	{
		stream.WriteByte((byte)obj.TypeId);

		return sizeof(byte) + obj.TypeId switch
		{
			TypeId.Nil => 0,
			TypeId.Number => stream.WriteNetDouble((Number)obj),
			TypeId.String => stream.WriteNetUtf8((CStr)obj),
			TypeId.Bool => stream.WriteBoolean((Bool)obj),
			TypeId.Array => stream.WriteCArray((ArrayBase)obj),
			TypeId.Table => stream.WriteTable((ReadOnlyTable)obj),
			TypeId.Vec2 => stream.WriteVec2((CVec2)obj),
			_ => throw new ArgumentException($"{obj.TypeId} is not serializable."),
		};
	}

	public static int WriteCArray(this Stream stream, ArrayBase value)
	{
		stream.WriteNetInt32(value.Count);

		int written = sizeof(int);
		
		foreach (var item in value)
		{
			written += stream.WriteCObject(item);
		}

		return written;
	}

	public static int WriteTable(this Stream stream, ReadOnlyTable value)
	{
		stream.WriteNetInt32(value.Count);
		
		int written = sizeof(int);

		foreach (var kvp in value)
		{
			written += stream.WriteCObject(kvp.Key);
			written += stream.WriteCObject(kvp.Value);
		}

		return written;
	}

	public static int WriteVec2(this Stream stream, Vec2 value)
	{
		return stream.WriteNetDouble(value.X) + stream.WriteNetDouble(value.Y);
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
		int utf16Bytes = stream.ReadNetInt32();

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

	public static CObj ReadCObject(this Stream stream)
	{
		var typeId = (TypeId)stream.ReadExactByte();

		return typeId switch
		{
			TypeId.Nil => Nil.Value,
			TypeId.Number => stream.ReadNetDouble(),
			TypeId.String => stream.ReadNetUtf8(),
			TypeId.Bool => stream.ReadBoolean(),
			TypeId.Array => stream.ReadCArray(),
			TypeId.Table => stream.ReadTable(),
			TypeId.Vec2 => stream.ReadVec2(),
			_ => throw new InvalidEnumArgumentException(nameof(typeId), (int)typeId, typeof(TypeId)),
		};
	}
	
	public static UserArray ReadCArray(this Stream stream)
	{
		int count = stream.ReadNetInt32();

		if (count < 0)
		{
			throw new InvalidDataException($"Array size (\'{count}\') was negative.");
		}

		var data = new List<CObj>(count);

		for (int i = 0; i < count; i++)
		{
			data.Add(stream.ReadCObject());
		}
		
		return new UserArray(data);
	}

	public static UserTable ReadTable(this Stream stream)
	{
		int count = stream.ReadNetInt32();

		if (count < 0)
		{
			throw new InvalidDataException($"Table size (\'{count}\') was negative.");
		}

		var data = new Dictionary<CObj, CObj>(count);

		for (int i = 0; i < count; i++)
		{
			var key = stream.ReadCObject();
			var value = stream.ReadCObject();

			if (!data.TryAdd(key, value))
			{
				throw new InvalidDataException($"Table contained duplicate key {key} for value {value}.");
			}
		}
		
		return new UserTable(data);
	}

	public static Vec2 ReadVec2(this Stream stream)
	{
		var x = stream.ReadNetDouble();
		var y = stream.ReadNetDouble();
		
		return new Vec2(x, y);
	}
}