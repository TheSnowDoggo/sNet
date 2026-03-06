using System.Collections.Frozen;
using System.Diagnostics;

namespace sNet.CScriptPro;

public sealed class TimePackage : Package
{
	public static readonly ReadOnlyTable Exports = new Dictionary<Obj, Obj>()
	{
		{ "unix", GlobalFunction.Create(Unix, 0, 1, TypeId.String) },
		{ "wait", GlobalFunction.Create(Wait, 1, 2, TypeId.Number, TypeId.String) },
	}.ToFrozenDictionary();

	public override string Name => "Time";
	public override Obj Export => Exports;

	private static Number Unix(Obj[] args)
	{
		var unix = DateTime.Now - DateTime.UnixEpoch;
		string prefix = args.Length > 0 ? (StrObj)args[0] : "s";
		
		return prefix switch
		{
			"d" => unix.TotalDays,
			"h" => unix.TotalHours,
			"m" => unix.TotalMinutes,
			"s" => unix.TotalSeconds,
			"ms" => unix.TotalMilliseconds,
			"us" => unix.Microseconds,
			"ns" => unix.TotalNanoseconds,
			_ => throw new ArgumentException($"Unrecognised time prefix {prefix}, expected (d, h, n, s, ms, us, ns)."),
		};
	}

	private static Obj Wait(Obj[] args)
	{
		double period = (Number)args[0];
		string prefix = args.Length > 1 ? (StrObj)args[1] : "ms";

		var sw = Stopwatch.StartNew();

		while (GetUnit(sw.Elapsed, prefix) < period) { }

		return Nil.Value;
	}

	private static double GetUnit(TimeSpan span, string prefix) => prefix switch
	{
		"d" => span.TotalDays,
		"h" => span.TotalHours,
		"m" => span.TotalMinutes,
		"s" => span.TotalSeconds,
		"ms" => span.TotalMilliseconds,
		"us" => span.Microseconds,
		"ns" => span.TotalNanoseconds,
		_ => throw new ArgumentException($"Unrecognised time prefix {prefix}, expected (d, h, n, s, ms, us, ns)."),
	};
}