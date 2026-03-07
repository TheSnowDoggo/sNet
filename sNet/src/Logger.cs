using System.Runtime.CompilerServices;
using System.Text;

namespace sNet;

public static class Logger
{
	private const string DirectoryName = "logs";
	
	private static int _isInitialized;
	private static FileStream _fileStream;
	private static StreamWriter _streamWriter;
	
	public static TextWriter Out { get; set; } = Console.Out;
	
	public static string LastError { get; private set; }
	public static string LastInfo { get; private set;}

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
	
	public static void Info(string message)
	{
		Log($"[INFO] {message}");
		
		LastInfo = message;
	}

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