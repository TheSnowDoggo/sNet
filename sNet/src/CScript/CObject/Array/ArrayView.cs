namespace sNet.CScriptPro;

public sealed class ArrayView<T> : ArrayBase
	where T : CObj
{
	private readonly IReadOnlyList<T> _list;

	public ArrayView(IReadOnlyList<T> list)
	{
		_list = list;
	}
	
	public override int Count => _list.Count;

	public override CObj this[int index] => _list[index];

	public override IEnumerator<CObj> GetEnumerator()
	{
		return _list.GetEnumerator();
	}
}