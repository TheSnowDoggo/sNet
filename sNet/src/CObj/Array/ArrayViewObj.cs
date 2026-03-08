namespace sNet.CScriptPro;

public sealed class ArrayViewObj<T> : ArrayBaseObj
	where T : Obj
{
	private readonly IReadOnlyList<T> _list;

	public ArrayViewObj(IReadOnlyList<T> list)
	{
		_list = list;
	}
	
	public override int Count => _list.Count;

	public override Obj this[int index] => _list[index];

	public override IEnumerator<Obj> GetEnumerator()
	{
		return _list.GetEnumerator();
	}
}