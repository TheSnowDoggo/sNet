using sNet.Client;

namespace sNet.Service.Chat;

public sealed class ClientChatService : ClientService
{
	public event Action<string> ChatReceived;

	public override ServiceId ServiceId => ServiceId.Chat;

	public override void Receive(NetCall call)
	{
		var sid = (ChatSid)call.Stream.ReadExactByte();

		switch (sid)
		{
		case ChatSid.Chat:
			HandleChat(call);
			break;
		default:
			Logger.Error($"Unrecognised chat sid: {sid}");
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
		try
		{
			string message = call.Stream.ReadNetUtf8();

			Logger.Info(message);

			ChatReceived?.Invoke(message);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
		}
	}
}