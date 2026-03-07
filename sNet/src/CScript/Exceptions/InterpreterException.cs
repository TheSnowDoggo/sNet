namespace sNet.CScriptPro;

public sealed class InterpreterException : Exception
{
    public InterpreterException(int line, string message)
        : base($"[Interpreter] Error at line {line}: {message}") { }
    
    public InterpreterException(string message)
	    : base($"[Interpreter] Error: {message}") { }
}