using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace sNet;

public static class SerialExtensions
{
	public static void WriteNetInt32(this Stream stream, int value)
	{
		Span<byte> span = stackalloc byte[sizeof(int)];
		
		int network = IPAddress.HostToNetworkOrder(value);
		
		Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), network);
		
		stream.Write(span);
	}
	
	public static void WriteNetUInt32(this Stream stream, uint value)
	{
		Span<byte> span = stackalloc byte[sizeof(uint)];
		
		int network = IPAddress.HostToNetworkOrder(Unsafe.BitCast<uint, int>(value));
		
		Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), network);
		
		stream.Write(span);
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

	public static byte ReadExactByte(this Stream stream)
	{
		int value = stream.ReadByte();

		if (value == -1)
		{
			throw new EndOfStreamException("Tried to read byte but stream was empty.");
		}
		
		return (byte)value;
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
		Span<byte> span = stackalloc byte[sizeof(uint)];
		
		stream.ReadExactly(span);

		int network = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(span));
		
		return Unsafe.BitCast<int, uint>(network);
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

		return string.Create(utf16Bytes, (buffer, utf8Bytes), (span, state) =>
		{
			Utf8.ToUtf16(state.buffer, span, out _, out _, replaceInvalidSequences: false);
		});
	}
}