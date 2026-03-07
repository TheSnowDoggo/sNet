namespace sNet;

public abstract class ServiceBase
{
	public abstract ServiceId ServiceId { get; }

	public virtual void Initialize()
	{
	}
	
	protected RentBuffer Format<T>((byte Sid, T Data) state)
		where T : INetSerializable
	{
		var buffer = RentBuffer.Share(sizeof(int) + 2 * sizeof(byte) + state.Data.MaxSize);

		try
		{
			using var serial = buffer.OpenSerial();
            
			serial.Begin();
            
			serial.WriteByte((byte)ServiceId);
			serial.WriteByte(state.Sid);
			state.Data.Serialize(serial);
            
			serial.End();
			buffer.Trim(serial.WrittenBytes);
            
			return buffer;
		}
		catch
		{
			buffer.Dispose();
			throw;
		}
	}

	protected RentBuffer Format(byte sid)
	{
		var buffer = RentBuffer.Share(sizeof(int) + sizeof(byte));

		try
		{
			using var serial = buffer.OpenSerial();
			
			serial.Begin();
			
			serial.WriteByte((byte)ServiceId);
			serial.WriteByte(sid);
			
			serial.End();
			buffer.Trim(serial.WrittenBytes);
			
			return buffer;
		}
		catch
		{
			buffer.Dispose();
			throw;
		}
	}
}