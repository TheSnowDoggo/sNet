using System.Collections;

namespace sNet.CScriptPro;

public abstract class ArrayBaseObj : Obj,
	IReadOnlyList<Obj>
{
	public ArrayBaseObj() : base(TypeId.Array) { }

	public abstract int Count { get; }

	public override Obj this[Obj key]
	{
		get => key.TypeId switch
		{
			TypeId.String => (string)key == "length" ? Count : Nil.Value,
			TypeId.Number => GetOrNil((int)key),
			_ => Nil.Value,
		};
		set
		{
			if (key.TypeId != TypeId.Number)
			{
				return;
			}
			
			int index = (int)key;

			if (index >= 0 && index < Count)
			{
				this[index] = value;
			}
		}
	}

	public virtual Obj this[int index]
	{
		get => Nil.Value;
		set { }
	}

	private Obj GetOrNil(int index)
	{
		return index >= 0 && index < Count ? this[index] : Nil.Value;
	}
	
	public override string ToString()
	{
		return $"[{string.Join(", ", this)}]";
	}

	public abstract IEnumerator<Obj> GetEnumerator();
	
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}