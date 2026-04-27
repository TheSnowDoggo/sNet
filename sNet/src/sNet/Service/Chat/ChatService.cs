namespace sNet.Service.Chat;

public static class ChatService
{
	public const int MaxCharacters = 256;
	
	public static RentBuffer FormatMessage(string message)
	{
		int bytes = sizeof(int) + 2 * sizeof(byte) + message.MaxUtf8ByteCount();
		var buffer = RentBuffer.Share(bytes);

		try
		{
			using var serial = new NetSerializer(buffer.Open());
			
			serial.Begin();
			
			serial.WriteByte((byte)ServiceId.Chat);
			serial.WriteByte((byte)ChatSid.Chat);
			serial.WriteUtf8(message);
			
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