using System.Collections;

namespace sNet.CScriptPro;

public sealed class WeakSet<T> : IReadOnlyCollection<T>
	where T : class
{
	private readonly List<WeakReference<T>> _data = [];

	public WeakSet() { }

	public WeakSet(int capacity)
	{
		_data = new List<WeakReference<T>>(capacity);
	}
	
	public int Count => _data.Count;
	
	public int Capacity => _data.Count;

	public bool Add(T value)
	{
		if (Contains(value))
		{
			return false;
		}
		
		_data.Add(new WeakReference<T>(value));

		return true;
	}

	public bool Remove(T value)
	{
		for (int i = 0; i < _data.Count; i++)
		{
			if (!_data[i].TryGetTarget(out var target))
			{
				_data.RemoveAt(i--);
				continue;
			}

			if (!EqualityComparer<T>.Default.Equals(value, target))
			{
				continue;
			}
			
			_data.RemoveAt(i);
			return true;
		}

		return false;
	}

	public bool Contains(T value)
	{
		foreach (var item in this)
		{
			if (EqualityComparer<T>.Default.Equals(value, item))
			{
				return true;
			}
		}
		
		return false;
	}

	public int Cull()
	{
		int culled = 0;
		
		for (int i = 0; i < _data.Count; i++)
		{
			if (_data[i].TryGetTarget(out _))
			{
				continue;
			}
			
			_data.RemoveAt(i--);
			culled++;
		}
		
		return culled;
	}
	
	public void Clear()
	{
		_data.Clear();
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < _data.Count; i++)
		{
			if (!_data[i].TryGetTarget(out T value))
			{
				_data.RemoveAt(i--);
				continue;
			}
			
			yield return value;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}