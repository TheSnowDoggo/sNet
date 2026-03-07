using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class IOPackage : Package
{
	public static readonly ReadOnlyTable Exports = new Dictionary<Obj, Obj>()
	{
		{ "print", GlobalFunction.Create(Print, 0, -1) },
		{ "println", GlobalFunction.Create(PrintLine, 0, -1) },
		{ "read", GlobalFunction.Create(_ => Console.Read()) },
		{ "readln", GlobalFunction.Create(_ => Console.ReadLine() ?? StrObj.Empty) },
		{ "require", GlobalFunction.Create(Require, TypeId.String) },
	}.ToFrozenDictionary();
	
	public override string Name => "IO";
	public override Obj Export => Exports;

	public static TextWriter Out { get; set; } = Console.Out;

	private static void Print(Obj[] args)
	{
		Out.Write(string.Join<Obj>("", args));
	}
	
	private static void PrintLine(Obj[] args)
	{
		Out.WriteLine(string.Join<Obj>("", args));
	}

	private static Obj Require(Obj[] args)
	{
		var filepath = (string)args[0];
		
		var result = FunctionDefinition.ParseMain(filepath).Create().Run();
		
		return result;
	}
}