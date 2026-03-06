using sNet.Client;

namespace sNet.Services.Chat;

public sealed class ClientChatService : ClientService
{
	public ClientChatService() : base(ServiceId.Chat) { }

	public event Action<string> ChatReceived;
	
	public override void Receive(NetCall call)
	{
		var chatId = (ChatSid)call.Stream.ReadExactByte();

		switch (chatId)
		{
		case ChatSid.Chat:
			HandleChat(call);
			break;
		default:
			Logger.Error($"Unrecognised chat id: {chatId}.");
			break;
		}
	}

	public async Task<bool> SendChatAsync(string message)
	{
		try
		{
			using var buffer = ChatService.FormatMessage(message);
			return await Client.SendAsync(buffer);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
	}

	public void FireChat(string message)
	{
		Task.Run(() => SendChatAsync(message));
	}

	private void HandleChat(NetCall call)
	{
		string message = call.Stream.ReadNetUtf8();
		
		Logger.Info(message);
		
		ChatReceived?.Invoke(message);
	}
}