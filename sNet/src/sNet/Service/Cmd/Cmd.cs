using System.Text;
using sNet.Server;

namespace sNet.Service.Cmd;

public sealed class Cmd
{
	public const int AnyArgs = -1;
	
	private readonly Func<string[], RemoteClient, bool> _action;
	
	private Cmd(Func<string[], RemoteClient, bool> action)
	{
		_action = action;
	}
	
	public int MinArgs { get; init; }
	public int MaxArgs { get; init; }
	public Permission Permissions { get; init; }
	public bool Remote { get; init; }

	public string Name => _action.Method.Name;
	
	public static Cmd Create(Func<string[], RemoteClient, bool> action, int minArgs, int maxArgs, Permission permissions, bool remote = false)
	{
		if (minArgs < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(minArgs), minArgs, "Min arguments was negative.");
		}
		
		if (maxArgs != AnyArgs && minArgs > maxArgs)
		{
			throw new ArgumentOutOfRangeException(nameof(minArgs), minArgs, $"Min arguments cannot be greater than max of {maxArgs}.");
		}

		return new Cmd(action)
		{
			MinArgs = minArgs,
			MaxArgs = maxArgs,
			Permissions = permissions,
			Remote = remote,
		};
	}

	public static Cmd Create(Func<string[], RemoteClient, bool> action, int args, Permission permissions, bool remote = false)
	{
		return Create(action, args, args, permissions, remote);
	}
	
	public static string[] SplitArgs(string input)
	{
		var list = new List<string>();
		
		var sb = new StringBuilder();

		var delimiterStack = new Stack<char>();
		
		foreach (var c in input)
		{
			switch (c)
			{
			case ' ':
				if (delimiterStack.Count > 0)
				{
					sb.Append(c);
					break;
				}
				
				list.Add(sb.ToString());
				sb.Clear();
				
				break;
			case '"':
			case '\'':
				if (delimiterStack.TryPeek(out var delimiter) && delimiter == c)
				{
					delimiterStack.Pop();
				}
				else
				{
					delimiterStack.Push(c);
				}
				break;
			default:
				sb.Append(c);
				break;
			}
		}

		if (sb.Length > 0)
		{
			list.Add(sb.ToString());
		}

		return list.ToArray();
	}
	
	public bool TryInvoke(string[] args, RemoteClient client)
	{
		if (client == null && Remote)
		{
			Logger.Error($"Cmd {Name} requires a remote client.");
			return false;
		}
		
		if (client != null && !client.AuthorisedAll(Permissions))
		{
			Logger.Error($"Client {client} is not authorised to run cmd {Name}.");
			return false;
		}
		
		int realArgs = args.Length - 1;

		if (realArgs < MinArgs)
		{
			Logger.Error($"Cmd {Name} expected minimum of {MinArgs}, got {realArgs}.");
			return false;
		}

		if (MaxArgs != AnyArgs && realArgs > MaxArgs)
		{
			Logger.Error($"Cmd {Name} expected maximum of {MaxArgs}, got {realArgs}.");
			return false;
		}

		try
		{
			return _action.Invoke(args, client);
		}
		catch (Exception ex)
		{
			Logger.Error($"Cmd {_action.Method.Name} threw error during invoke: {ex.Message}");
			return false;
		}
	}
}