using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace sNet.CScriptPro;

public sealed class MathPackage : Package
{
	public static readonly ReadOnlyTable Exports = new Dictionary<Obj, Obj>()
	{
		{ "PI", Math.PI },
		{ "E", Math.E },
		{ "TAU", Math.Tau },
		
		{ "sqrt", CreateUnary(Math.Sqrt) },
		{ "cbrt", CreateUnary(Math.Cbrt) },
		
		{ "pow", CreateBinary(Math.Pow) },
		{ "exp", CreateUnary(Math.Exp) },
		{ "log10", CreateUnary(Math.Log10) },
		{ "log", CreateBinary(Math.Log) },
		{ "ln", CreateUnary(Math.Log) },
		{ "log2", CreateUnary(Math.Log2) },
		{ "ilog2", GlobalFunction.Create(Logb2, TypeId.Number) },
		
		{ "sin", CreateUnary(Math.Sin) },
		{ "cos", CreateUnary(Math.Cos) },
		{ "tan", CreateUnary(Math.Tan) },
		{ "arcsin", CreateUnary(Math.Asin) },
		{ "arccos", CreateUnary(Math.Acos) },
		{ "arctan", CreateUnary(Math.Atan) },
		
		{ "sinh", CreateUnary(Math.Sinh) },
		{ "cosh", CreateUnary(Math.Cosh) },
		{ "tanh", CreateUnary(Math.Tanh) },
		{ "arcsinh", CreateUnary(Math.Asinh) },
		{ "arccosh", CreateUnary(Math.Acosh) },
		{ "arctanh", CreateUnary(Math.Atanh) },
		
		{ "radToDeg", CreateUnary(RadToDeg) },
		{ "degToRad", CreateUnary(DegToRad) },
		
		{ "ceil", CreateUnary(Math.Ceiling) },
		{ "floor", CreateUnary(Math.Floor) },
		{ "truncate", CreateUnary(Math.Truncate) },
		{ "round", GlobalFunction.Create(Round, 1, 2, TypeId.Number, TypeId.Number) },
		
		{ "max", CreateBinary(Math.Max) },
		{ "min", CreateBinary(Math.Min) },
		{ "clamp", CreateTertiary(Math.Clamp) },
		{ "lerp", CreateTertiary(Lerp) },
		{ "unlerp", CreateTertiary(Unlerp) },
		
		{ "abs", CreateUnary(Math.Abs) },
		
		{ "isPrime", GlobalFunction.Create(IsPrime, TypeId.Number) },
		{ "getPrimes", GlobalFunction.Create(GetPrimes, TypeId.Number) },
	}.ToFrozenDictionary();
	
	public override string Name => "Math";
	public override Obj Export => Exports;

	private static GlobalFunction CreateUnary(Func<double, double> func)
	{
		return GlobalFunction.Create(Func, TypeId.Number);
		
		Number Func(Obj[] args)
		{
			return func.Invoke((double)(Number)args[0]);
		}
	}

	private static GlobalFunction CreateBinary(Func<double, double, double> func)
	{
		return GlobalFunction.Create(Func, TypeId.Number, TypeId.Number);

		Number Func(Obj[] args)
		{
			return func.Invoke((double)args[0], (double)args[1]);
		}
	}

	private static GlobalFunction CreateTertiary(Func<double, double, double, double> func)
	{
		return GlobalFunction.Create(Func, TypeId.Number, TypeId.Number, TypeId.Number);
		
		Number Func(Obj[] args)
		{
			return func.Invoke((double)args[0], (double)args[1], (double)args[2]);
		}
	}

	private static double RadToDeg(double rad)
	{
		return rad * 180 / Math.PI;
	}

	private static double DegToRad(double deg)
	{
		return deg * Math.PI / 180;
	}

	private static Number Round(Obj[] args)
	{
		int digits = args.Length > 1 ? (int)args[1] : 0;
		
		return Math.Round((double)args[0], digits);
	}
	
	private static Number Log(Obj[] args)
	{
		return Math.Log((double)args[0], (double)args[1]);
	}

	private static Number Logb2(Obj[] args)
	{
		return Math.ILogB((double)args[0]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double Lerp(double value, double min, double max)
	{
		return double.Lerp(min, max, value);
	}

	private static double Unlerp(double value, double min, double max)
	{
		return (value - min) / (max - min);
	}
	
	private static Bool IsPrime(Obj[] args)
	{
		long x = (long)args[0];

		if (x == 2)
		{
			return true;
		}

		if (x <= 1 || (x & 1) == 0)
		{
			return false;
		}

		long end = (long)Math.Sqrt(x);

		for (long i = 3; i <= end; i += 2)
		{
			if (x % i == 0)
			{
				return false;
			}
		}
		
		return true;
	}

	private static ArrayObj GetPrimes(Obj[] args)
	{
		var n = (long)args[0];

		if (n <= 1)
		{
			return new ArrayObj();
		}

		var primes = new List<Number>() { 2 };

		for (long i = 3; i <= n; i += 2)
		{
			var end = (long)Math.Sqrt(i);

			bool isPrime = true;
			
			for (int j = 1; j < primes.Count; j++)
			{
				var prime = (long)primes[j];
				
				if (prime > end)
				{
					break;
				}

				if (i % prime != 0)
				{
					continue;
				}
				
				isPrime = false;
				break;
			}

			if (isPrime)
			{
				primes.Add(i);
			}
		}

		return new ArrayObj(primes);
	}
}