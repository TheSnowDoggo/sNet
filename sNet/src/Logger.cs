using System.Runtime.CompilerServices;

namespace sNet;

public static class Logger
{
	public static TextWriter Out { get; } = Console.Out;
	
	public static void Info(string message)
	{
		Log($"[INFO] {message}");
	}

	public static void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
	{
		Log($"[ERROR] {Path.GetFileNameWithoutExtension(filePath)}.{memberName}(), {message}");
	}

	private static void Log(string content)
	{
		string message = $"{DateTime.Now:HH:mm:ss} {content}";
		
		Out.WriteLine(message);
	}
}