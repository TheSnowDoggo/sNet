using System.Buffers;

namespace sNet;

public sealed class RentBuffer : IDisposable
{
	private readonly ArrayPool<byte> _arrayPool;
	private byte[] _data;
	private int _end;
	private int _isDisposed;
	
	public RentBuffer(ArrayPool<byte> arrayPool, int minimumSize)
	{
		_arrayPool = arrayPool;
		_data = arrayPool.Rent(minimumSize);
		_end = minimumSize;
	}

	public byte[] Data
	{
		get
		{
			ObjectDisposedException.ThrowIf(_isDisposed != 0, this);
			return _data;
		}
	}

	/// <summary>
	/// Gets the suggested end of the buffer.
	/// </summary>
	public int End
	{
		get
		{
			ObjectDisposedException.ThrowIf(_isDisposed != 0, this);
			return _end;
		}
	}
	
	public int Length => Data.Length;
	
	public byte this[int index] => Data[index];
	
	public static implicit operator ArraySegment<byte>(RentBuffer buffer)
		=> new ArraySegment<byte>(buffer.Data, 0, buffer.End);
	
	public static implicit operator ReadOnlySpan<byte>(RentBuffer buffer)
		=> new ReadOnlySpan<byte>(buffer.Data, 0, buffer.End);
	
	public static implicit operator Span<byte>(RentBuffer buffer)
		=> new Span<byte>(buffer.Data, 0, buffer.End);

	public static RentBuffer Share(int minSize)
	{
		return new RentBuffer(ArrayPool<byte>.Shared, minSize);
	}

	public MemoryStream Open(bool writable = true)
	{
		return new MemoryStream(Data, 0, End, writable);
	}

	public NetSerializer OpenSerial()
	{
		return new NetSerializer(Open());
	}

	public MemoryStream OpenRead()
	{
		return Open(false);
	}

	public void Trim(int newSize)
	{
		ObjectDisposedException.ThrowIf(_isDisposed != 0, this);
		
		if (newSize < 0 || newSize >= Data.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(newSize), newSize, "New size was invalid.");
		}
		
		_end = newSize;
	}

	public void Dispose()
	{
		if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
		{
			return;
		}
		
		_arrayPool.Return(_data);
		_data = null;
		_end = -1;
	}
}