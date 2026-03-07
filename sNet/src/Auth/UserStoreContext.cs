using System.Text.Json.Serialization;
using sNet.Server;

namespace sNet.Auth;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonConverter(typeof(JsonStringEnumConverter<Permission>))]
[JsonSerializable(typeof(UserStore))]
public sealed partial class UserStoreContext : JsonSerializerContext;