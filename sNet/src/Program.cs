using System.Diagnostics;
using sNet.CScriptPro;
using sNet.Client;
using sNet.Server;
using sNet.Service.Chat;
using sNet.Service.Part;

namespace sNet;

internal static class Program
{
	private static void Main()
	{
		var server = new NetServer(2);
		server.Services.Add(new ServerChatService());

		var serverPart = new ServerPartService();

		var serverRoot = new ServerPartRoot()
		{
			Service = serverPart,
		};
		
		server.Services.Add(serverPart);

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

		var clientRoot = new ClientPartRoot();

		var clientPart = new ClientPartService()
		{
			Root = clientRoot,
		};
		
		client.Services.Add(clientPart);

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

		serverRoot.Root.AddChild(PartTag.Parse("scripts/base.part").Create());

		var sw = Stopwatch.StartNew();
		
		while (true)
		{
			serverRoot.Update(0);
			
			var part = serverRoot.Root.FindFirstChild("Test");
			part["position"] = (Vector2)part["position"] + Vector2.Up;
			clientRoot.Update(0);

			Console.WriteLine(clientRoot.Root.FindFirstChild("Test"));

			while (sw.Elapsed.TotalSeconds < 1.0 / 60)
			{
			}
			
			sw.Restart();
		}
	}

	private static void SerialTest()
	{
		var function = FunctionDefinition.ParseMain("scripts/script.csrp").Create();
		
		var value = function.Run();

		using var buffer = RentBuffer.Share(4096);

		using (var serial = new NetSerializer(buffer.Open()))
		{
			serial.WriteCObj(value);
			buffer.Trim(serial.WrittenBytes);
		}

		using (var stream = buffer.OpenRead())
		{
			value = stream.ReadCObj();
		}

		Console.WriteLine(value);

		Console.Read();
	}
}