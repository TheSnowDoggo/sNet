using sNet.Server;

namespace sNet.Services.Chat;

public sealed class ServerChatService : IServerService
{
	public ServerChatService() : base(ServiceId.Chat) { }

	public override void Receive(ServerNetCall call)
	{
		var chatId = (ChatId)call.Stream.ReadByte();

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

	private void HandleChat(ServerNetCall call)
	{
		string content = call.Stream.ReadNetUtf8();

		string message = $"<{call.Client.Idx}> {content}";

		Task.Run(() => BroadcastMessageAsync(message, call.Client.Idx));
	}

	private async Task BroadcastMessageAsync(string message, int exclude)
	{
		try
		{
			
			
			await Server.BroadcastAsync(buffer, [exclude]);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
		}
	}
}