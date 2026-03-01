using sNet.Client;

namespace sNet.Services.Chat;

public sealed class ClientChatService : ClientService
{
	public ClientChatService() : base(ServiceId.Chat) { }

	public event Action<string> ChatReceived;
	
	public override void Receive(NetCall call)
	{
		var chatId = (ChatId)call.Stream.ReadByte();

		switch (chatId)
		{
		case ChatId.Chat:
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

		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
			return false;
		}
	}

	private void HandleChat(NetCall call)
	{
		string message = call.Stream.ReadNetUtf8();
		
		Logger.Info(message);
		
		ChatReceived?.Invoke(message);
	}
}