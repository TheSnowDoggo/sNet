namespace sNet.Service.Cmd;

public sealed class ClientCmdService : ClientService
{
	private const int MaxSendSize = 512;

	public event Action<string> ResponseReceived;
	
	public override ServiceId ServiceId => ServiceId.Cmd;

	public override void Receive(NetCall call)
	{
		var sid = (CmdSid)call.Stream.ReadExactByte();

		switch (sid)
		{
		case CmdSid.SendResponse:
			HandleResponse(call);
			break;
		default:
			Logger.Error($"Unrecognised cmd sid: {sid}.");
			break;
		}
	}

	public async Task<bool> SendCmdAsync(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			Logger.Error("Input cannot be null or whitespace.");
			return false;
		}

		if (input.Length > MaxSendSize)
		{
			Logger.Error($"Input too long. Max size is {MaxSendSize}.");
			return false;
		}
		
		return await SendAsync((byte)CmdSid.RequestRun, new SerialString(input));
	}

	public void FireCmd(string input)
	{
		Task.Run(() => SendCmdAsync(input));
	}

	private void HandleResponse(NetCall call)
	{
		var response = call.Stream.ReadNetUtf8();
		
		Logger.Info($"Response: {response}");
		
		ResponseReceived?.Invoke(response);
	}
}