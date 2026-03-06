using System.Runtime.CompilerServices;

namespace sNet.CScriptPro;

public sealed class ParserException : Exception
{
	public ParserException(int line, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filepath = "")
		: base($"[Parser] At {Path.GetFileNameWithoutExtension(filepath)}.{memberName}(), Error at line {line}: {message}") { }
}