using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class Rand : Obj
{
	private readonly Random _random;

	public static readonly ReadOnlyTable Export = new Dictionary<Obj, Obj>()
	{
		{ "new", GlobalFunction.Create(New, 0, 1, TypeId.Number) },
	}.ToFrozenDictionary();

	public Rand()
	{
		_random = new Random();
	}
	
	public Rand(int seed)
	{
		_random = new Random(seed);
	}

	public override TypeId TypeId => TypeId.Rand;

	public override Obj this[Obj key] => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
	{
		"nextInt" => GlobalFunction.Create(NextInt, 0, 2, TypeId.Number, TypeId.Number),
		"nextReal" => GlobalFunction.Create(NextReal, 0, 2, TypeId.Number, TypeId.Number),
		_ => Nil.Value,
	};

	private static Rand New(Obj[] args)
	{
		return args.Length == 0 ? new Rand() : new Rand((int)args[0]);
	}

	private Number NextInt(Obj[] args) => args.Length switch
	{
		0 => _random.NextInt64(),
		1 => _random.NextInt64((int)args[0]),
		2 => _random.NextInt64((int)args[0], (int)args[1]),
		_ => throw new ArgumentOutOfRangeException(nameof(args), args, $"Expected 0, 1 or 2 arguments, got {args.Length}"),
	};

	private Number NextReal(Obj[] args) => args.Length switch
	{
		0 => _random.NextDouble(),
		1 => _random.NextDouble((double)args[0]),
		2 => _random.NextDouble((double)args[0], (double)args[1]),
		_ => throw new ArgumentOutOfRangeException(nameof(args), args, $"Expected 0, 1 or 2 arguments, got {args.Length}"),
	};
}