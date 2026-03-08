using System.Text.Json.Serialization;

namespace sNet.Server;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ServerConfig))]
public sealed partial class ServerConfigContext : JsonSerializerContext;