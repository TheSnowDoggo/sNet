using CScriptPro;
using sNet.Client;
using sNet.Server;
using sNet.Services.Chat;

namespace sNet;

internal static class Program
{
	private static void Main()
	{
		const string Path = @"C:\Users\redst\RiderProjects\sNet\sNet\script.csrp";

		var function = FunctionDefinition.ParseMain(Path).Create();
		
		using var buffer = RentBuffer.Share(4096);

		using (var serial = new NetSerializer(buffer.Open()))
		{
			serial.WriteCObject(function.Run());
			buffer.Trim(serial.WrittenBytes);
		}

		using (var serial = buffer.OpenRead())
		{
			var obj = serial.ReadCObject();

			Console.WriteLine(obj);
		}
		
		return;
		
		var server = new NetServer(2);
		server.Services.Add(new ServerChatService());

		if (!server.Bind())
		{
			return;
		}

		if (!server.Start())
		{
			return;
		}
		
		var client = new NetClient();
		client.Services.Add(new ClientChatService());

		var connect = client.ConnectAsync("192.168.0.157", server.Port);
		connect.Wait();
		if (!connect.Result)
		{
			return;
		}

		if (!client.Start())
		{
			return;
		}
		
		var chat = client.Services.Get<ClientChatService>(ServiceId.Chat);

		chat.ChatReceived += message => chat.FireChat($"I got: {message}");
		
		server.Services.Get<ServerChatService>(ServiceId.Chat).FireBroadcast("hello");

		Console.ReadLine();
	}
}