using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using sNet.Server;

namespace sNet.Auth;

public sealed class UserStore
{
	public UserStore(string filepath)
	{
		Filepath = filepath;
	}
	
	public readonly Dictionary<string, User> Users = [];
	
	[JsonIgnore]
	public string Filepath { get; }
	
	[JsonIgnore]
	public static UserStore Current { get; set; }

	public static bool TryLoadOrCreate(string filepath, out UserStore userStore)
	{
		try
		{
			if (!File.Exists(filepath))
			{
				userStore = new UserStore(filepath);
				
				Logger.Info($"Created user store at {filepath}");
				
				return userStore.TrySave();
			}

			using var fs = File.OpenRead(filepath);
			userStore = JsonSerializer.Deserialize<UserStore>(fs, UserStoreContext.Default.UserStore);

			if (userStore != null)
			{
				return true;
			}
			
			Logger.Error("Failed to load user store: Result was null.");
			return false;
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to load user store from {filepath}: {ex.Message}");
			userStore = null;
			return false;
		}
	}

	public static string HashPassword(string password)
	{
		using var buffer = RentBuffer.Share(password.Length * 3);

		Utf8.FromUtf16(password, buffer, out _, out int written);
		
		buffer.Trim(written);

		using var result = RentBuffer.Share(SHA256.HashSizeInBytes);

		SHA256.HashData(buffer, result);
		
		return Convert.ToHexStringLower(result);
	}
	
	public bool TrySave(FileMode fileMode = FileMode.Create)
	{
		try
		{
			using var fs = File.Open(Filepath, fileMode, FileAccess.Write);
			JsonSerializer.Serialize(fs, this, UserStoreContext.Default.UserStore);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to save tp {Filepath}: {ex.Message}");
			return false;
		}
	}

	public bool Create(string username, string password, Permission permissions)
	{
		if (password.Length is < 3 or > 32)
		{
			Logger.Error("Password must be between 3 and 32 characters long.");
			return false;
		}
		
		var user = new User()
		{
			Password = HashPassword(password),
			Permissions = permissions,
		};

		if (!Users.TryAdd(username, user))
		{
			Logger.Error($"User {username} already exists.");
			return false;
		}
		
		return true;
	}

	public bool TryLogin(string username, string password, out User user)
	{
		user = null;
		
		if (!Users.TryGetValue(username, out var attemptedUser))
		{
			Logger.Error($"User with username {username} not found.");
			return false;
		}

		if (HashPassword(password) != attemptedUser.Password)
		{
			Logger.Error("Invalid password provided.");
			return false;
		}

		user = attemptedUser;
		return true;
	}
}