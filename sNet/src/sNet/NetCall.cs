using Serial;

namespace sNet;

public class NetCall : IDisposable
{
	private readonly RentBuffer _buffer;
	private readonly Stream _stream;
	
	private int _isDisposed;
	
	public NetCall(RentBuffer buffer)
	{
		_buffer = buffer;
		_stream = buffer.Open();
	}

	public RentBuffer Buffer
	{
		get
		{
			ObjectDisposedException.ThrowIf(_isDisposed != 0, this);
			return _buffer;
		}
	}

	public Stream Stream
	{
		get
		{
			ObjectDisposedException.ThrowIf(_isDisposed != 0, this);
			return _stream;
		}
	}

	public int End => _buffer.End;
	
	public void Dispose()
	{
		if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
		{
			return;
		}
		
		GC.SuppressFinalize(this);
		
		_buffer.Dispose();
		_stream.Dispose();
	}
}