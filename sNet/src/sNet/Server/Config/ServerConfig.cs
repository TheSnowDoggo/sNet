using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sNet.Server;

public sealed class ServerConfig
{
	public ServerConfig(string filepath = null)
	{
		Filepath = filepath;
	}
	
	public int Port { get; set; } = 17324;
	public int MaxClients { get; set; } = 10;
	public int MaxClientBacklog { get; set; } = 10;
	
	[JsonIgnore]
	public string Filepath { get; set; }

	public static bool TryLoadOrCreate(string filepath, [NotNullWhen(true)] out ServerConfig config)
	{
		try
		{
			if (!File.Exists(filepath))
			{
				config = new ServerConfig(filepath);
				Logger.Info($"Creating server config at {filepath}.");
				return config.TrySave(FileMode.CreateNew);
			}
			
			using var fs = File.OpenRead(filepath);
			config = JsonSerializer.Deserialize(fs, ServerConfigContext.Default.ServerConfig);

			if (config != null)
			{
				config.Filepath = filepath;
				return true;
			}
			
			Logger.Error($"Failed to load config from {filepath}: Result was null.");
			return false;
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to load config from {filepath}: {ex.Message}");
			config = null;
			return false;
		}
	}
	
	public bool TrySave(FileMode fileMode = FileMode.Create)
	{
		try
		{
			using var fs = File.Open(Filepath, fileMode, FileAccess.Write);
			JsonSerializer.Serialize(fs, this, ServerConfigContext.Default.ServerConfig);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error($"Error saving config to {Filepath}: {ex.Message}");
			return false;
		}
	}
}