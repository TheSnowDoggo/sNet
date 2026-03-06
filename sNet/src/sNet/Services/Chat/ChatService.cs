namespace sNet.Services.Chat;

public static class ChatService
{
	public static RentBuffer FormatMessage(string message)
	{
		int bytes = sizeof(int) + 2 * sizeof(byte) + message.Length * 3;
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