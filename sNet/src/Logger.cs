using System.Runtime.CompilerServices;
using System.Text;

namespace sNet;

/// <summary>
/// Provides error and info logging utilities.
/// </summary>
public static class Logger
{
	private const string DirectoryName = "logs";
	
	private static int _isInitialized;
	private static FileStream _fileStream;
	private static StreamWriter _streamWriter;
	
	/// <summary>
	/// Gets or sets the current console output text writer.
	/// </summary>
	public static TextWriter Out { get; set; } = Console.Out;
	
	/// <summary>
	/// Gets the last error message or null if no errors have been logged.
	/// </summary>
	public static string LastError { get; private set; }
	
	/// <summary>
	/// Gets the last info message or null if no infos have been logged.
	/// </summary>
	public static string LastInfo { get; private set;}

	/// <summary>
	/// Initializes the internal log file.
	/// </summary>
	public static void Initialize()
	{
		if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) != 0)
		{
			Error("Cannot initialize logger: Already initialized.");
			return;
		}

		try
		{
			Directory.CreateDirectory(DirectoryName);
			
			string name = Path.Combine(DirectoryName, $"log-{DateTime.Now:yyyy-MM-dd}.log");

			_fileStream = File.Open(name, FileMode.Append, FileAccess.Write);
			_streamWriter = new StreamWriter(_fileStream, Encoding.UTF8);
		}
		catch (Exception ex)
		{
			Error($"Cannot initialize logger: {ex.Message}");
			Interlocked.Exchange(ref _isInitialized, 0);
		}
	}

	/// <summary>
	/// Closes the internal log file.
	/// </summary>
	public static void Close()
	{
		if (Interlocked.CompareExchange(ref _isInitialized, 0, 1) == 0)
		{
			Error("Cannot close logger: Not initialized.");
			return;
		}
		
		_fileStream?.Close();
		_streamWriter = null;
	}
	
	/// <summary>
	/// Logs an info message.
	/// </summary>
	/// <param name="message">The message content.</param>
	public static void Info(string message)
	{
		Log($"[INFO] {message}");
		
		LastInfo = message;
	}

	/// <summary>
	/// Logs an error message.
	/// </summary>
	/// <param name="message">The message content.</param>
	/// <param name="memberName">The caller member name.</param>
	/// <param name="filePath">The caller file path.</param>
	public static void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
	{
		Log($"[ERROR] {Path.GetFileNameWithoutExtension(filePath)}.{memberName}(), {message}");
		
		LastError = message;
	}

	private static void Log(string content)
	{
		string message = $"{DateTime.Now:HH:mm:ss} {content}";
		
		Out?.WriteLine(message);
		
		_streamWriter?.WriteLine(message);
		_streamWriter?.Flush();
	}
}