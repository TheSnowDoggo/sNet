using sNet.Server;
using Serial;

namespace sNet.Services.Chat;

public sealed class ServerChatService : ServerService
{
	public ServerChatService() : base(ServiceId.Chat) { }

	public override void Receive(ServerNetCall call)
	{
		var chatId = (ChatId)call.Stream.ReadExactByte();

		switch (chatId)
		{
		case ChatId.Chat:
			HandleChat(call);
			break;
		default:
			Logger.Error($"Unrecognised chat id {chatId}.");
			break;
		}
	}
	
	public async Task<bool> BroadcastMessageAsync(string message, int exclude = -1)
	{
		try
		{
			using var buffer = ChatService.FormatMessage(message);
			return await Server.BroadcastAsync(buffer, exclude == -1 ? null : [exclude]);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
	}

	public void FireBroadcast(string message, int exclude = -1)
	{
		Task.Run(() => BroadcastMessageAsync(message, exclude));
	}

	public async Task<bool> SendChatAsync(int idx, string message)
	{
		try
		{
			using var buffer = ChatService.FormatMessage(message);
			return await Server.SendAsync(idx, buffer);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			return false;
		}
	}

	public void FireChat(int idx, string message)
	{
		Task.Run(() => SendChatAsync(idx, message));
	}

	private void HandleChat(ServerNetCall call)
	{
		string content = call.Stream.ReadNetUtf8();

		string message = $"<{call.Client.Idx}> {content}";
		
		Logger.Info(message);

		Task.Run(() => BroadcastMessageAsync(message, call.Client.Idx));
	}
}