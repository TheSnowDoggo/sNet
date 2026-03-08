using System.Collections.Frozen;
using System.ComponentModel;
using System.Text;

namespace sNet.CScriptPro;

public class Part : Obj
{
	private readonly List<Part> _children = [];

	public static readonly FrozenDictionary<string, IProperty> GlobalProperties = new Dictionary<string, IProperty>()
	{
		{ "id", new GProperty<Part>(p => (StrObj)p.PartType.ToString()) },
		{ "uid", new GProperty<Part>(p => (UidObj)p.Uid) },
		{ "name", new GSProperty<Part, StrObj>(p => p.Name, (p, v) => p.Name = v, TypeId.String) },
		{ "enabled", new GSProperty<Part, Bool>(p => p.Enabled, (p, v) => p.Enabled = v, TypeId.Bool) },
		{ "visible", new GSProperty<Part, Bool>(p => p.Visible, (p, v) => p.Visible = v, TypeId.Bool) },
		{ "parent", new GProperty<Part>(p => (Obj)p.Parent ?? Nil.Value) },
		{ "children", new GProperty<Part>(p => new ArrayViewObj<Part>(p._children)) },
		{ "addChild", new GProperty<Part>(p => GlobalFunction.Create(args => p.AddChild((Part)args[0]), TypeId.Part)) },
		{ "removeChild", new GProperty<Part>(p => GlobalFunction.Create(args => p.RemoveChild((Part)args[0]), TypeId.Part)) },
		{ "findFirstChild", new GProperty<Part>(p => GlobalFunction.Create(args => p.FindFirstChild((StrObj)args[0]), TypeId.String)) },
	}.ToFrozenDictionary();
	
	public static readonly ReadOnlyTable Export = new Dictionary<Obj, Obj>()
	{
		{ "new", GlobalFunction.Create(New, TypeId.String) },
		{ "load", GlobalFunction.Create(Load, TypeId.String) },
	}.ToFrozenDictionary();

	public override TypeId TypeId => TypeId.Part;

	public virtual PartType PartType => PartType.Part;
	
	public IReadOnlyList<Part> Children => _children.AsReadOnly();

	public StrObj Name { get; set; } = StrObj.Empty;
	public Bool Enabled { get; set; } = true;
	public Bool Visible { get; set; } = true;
	
	public Uid Uid { get; set; }
	
	public PartRoot Root { get; set; }
	
	public Part Parent { get; private set; }

	public virtual IReadOnlyDictionary<string, IProperty> Properties => GlobalProperties;
	
	public override Obj this[Obj key]
	{
		get
		{
			if (key.TypeId != TypeId.String)
			{
				return Nil.Value;
			}

			var name = (string)key;

			if (!Properties.TryGetValue(name, out IProperty property))
			{
				return FindFirstChild(name);
			}

			return property[this];
		}
		set
		{
			if (key.TypeId != TypeId.String)
			{
				return;
			}

			var name = (string)key;

			if (!Properties.TryGetValue(name, out IProperty property))
			{
				return;
			}

			if (!property.Serializable)
			{
				property[this] = value;
				return;
			}

			if (EqualityComparer<Obj>.Default.Equals(property[this], value))
			{
				return;
			}

			property[this] = value;
			Root?.PropertyUpdate(this, name, value);
		}
	}

	public static Part Create(PartType partType) => partType switch
	{
		PartType.Part => new Part(),
		PartType.Part2d => new Part2d(),
		PartType.Script => new Script(),
		PartType.Camera2d => new Camera2d(),
		PartType.Sprite2d => new Sprite2d(),
		PartType.Box2d => new Box2d(),
		_ => throw new InvalidEnumArgumentException(nameof(partType), (int)partType, typeof(PartType)),
	};

	public static Part DeserializeNew(Stream stream)
	{
		var partType = (PartType)stream.ReadByte();

		var part = Create(partType);
		part.Deserialize(stream);
		return part;
	}

	public IEnumerable<Part> Descendants(SearchMode mode = SearchMode.Breadth) => mode switch
	{
		SearchMode.Breadth => DescendentsBreadth(),
		SearchMode.Depth => DescendentsDepth(),
		_ => throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(SearchMode)),
	};
	
	public bool AddChild(Part child)
	{
		if (_children.Contains(child))
		{
			return false;
		}

		if (child.Parent != null)
		{
			return false;
		}

		child.Parent = this;
		_children.Add(child);

		ParentUpdate();
		
		Root?.PartAdded(child);
		
		return true;
	}

	public bool RemoveChild(Part child)
	{
		return RemoveChild(_children.IndexOf(child));
	}

	public bool RemoveChild(int idx)
	{
		if (idx < 0 || idx >= _children.Count)
		{
			return false;
		}
		
		var child = _children[idx];
		
		_children.RemoveAt(idx);
		
		child.Parent = null;
		ParentUpdate();
		
		Root?.PartRemoved(child);

		return true;
	}

	public Obj FindFirstChild(StrObj name)
	{
		foreach (var child in _children)
		{
			if (child.Name == name)
			{
				return child;
			}
		}

		return Nil.Value;
	}

	public override string ToString()
	{
		var sb = new StringBuilder();
		Format(sb, 0);
		return sb.ToString();
	}

	public int Serialize(Stream stream)
	{
		int bytes = stream.WriteNetByte((byte)PartType);

		bytes += stream.WriteNetInt64(Uid);

		foreach (var property in Properties.Values)
		{
			if (!property.Serializable)
			{
				continue;
			}
			
			bytes += property.Serialize(this, stream);
		}

		bytes += stream.WriteNetInt32(_children.Count);
		
		foreach (var child in _children)
		{
			bytes += child.Serialize(stream);
		}
		
		return bytes;
	}

	public void Deserialize(Stream stream)
	{
		Uid = stream.ReadNetInt64();
		
		foreach (var property in Properties.Values)
		{
			if (!property.Serializable)
			{
				continue;
			}
			
			property.Deserialize(this, stream);
		}

		int count = stream.ReadNetInt32();

		if (count < 0)
		{
			throw new InvalidDataException($"Child count (\'{count}\') was negative.");
		}

		for (int i = 0; i < count; i++)
		{
			AddChild(DeserializeNew(stream));
		}
	}

	public void ClearChildren()
	{
		for (int i = _children.Count - 1; i >= 0; i--)
		{
			RemoveChild(i);
		}
	}

	protected virtual void ParentUpdate()
	{
	}

	private static Part New(Obj[] args)
	{
		var partStr = (string)args[0];

		if (!Enum.TryParse<PartType>(partStr, true, out var partId))
		{
			throw new ArgumentException($"Unrecognised part id {partStr}.");
		}

		return Create(partId);
	}

	private static Part Load(Obj[] args)
	{
		var filepath = (string)args[0];
		
		var tag = PartTag.Parse(filepath);

		return tag.Create();
	}

	private IEnumerable<Part> DescendentsBreadth()
	{
		var queue = new Queue<Part>();
		queue.Enqueue(this);

		while (queue.TryDequeue(out var part))
		{
			yield return part;

			foreach (var child in part._children)
			{
				queue.Enqueue(child);
			}
		}
	}
	
	private IEnumerable<Part> DescendentsDepth()
	{
		var stack = new Stack<Part>();
		stack.Push(this);

		while (stack.TryPop(out var part))
		{
			yield return part;

			foreach (var child in part._children)
			{
				stack.Push(child);
			}
		}
	}

	private void Format(StringBuilder sb, int level)
	{
		sb.Append(' ', level * 2);
		sb.AppendLine($"{PartType} {{");

		foreach (var property in Properties)
		{
			sb.Append(' ', (level + 1) * 2);
			sb.AppendLine($"{property.Key}: {property.Value[this]},");
		}
		
		foreach (var child in _children)
		{
			child.Format(sb, level + 1);
			sb.AppendLine(",");
		}

		sb.Append(' ', level * 2);
		sb.Append('}');
	}
}