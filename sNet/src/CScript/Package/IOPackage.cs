using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class IOPackage : Package
{
	public static readonly ReadOnlyTable Exports = new Dictionary<CObj, CObj>()
	{
		{ "print", GlobalFunction.Create(Print, 0, -1) },
		{ "println", GlobalFunction.Create(PrintLine, 0, -1) },
		{ "read", GlobalFunction.Create(_ => Console.Read()) },
		{ "readln", GlobalFunction.Create(_ => Console.ReadLine() ?? CStr.Empty) },
		{ "require", GlobalFunction.Create(Require, TypeId.String) },
	}.ToFrozenDictionary();
	
	public override string Name => "IO";
	public override CObj Export => Exports;

	private static void Print(CObj[] args)
	{
		Console.Write(string.Join<CObj>("", args));
	}
	
	private static void PrintLine(CObj[] args)
	{
		Console.WriteLine(string.Join<CObj>("", args));
	}

	private static CObj Require(CObj[] args)
	{
		string filepath = (string)(CStr)args[0];
		
		var result = FunctionDefinition.ParseMain(filepath).Create().Run();
		
		return result;
	}
}