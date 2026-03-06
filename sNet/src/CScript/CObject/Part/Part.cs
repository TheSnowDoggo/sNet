using System.Collections.Frozen;
using System.ComponentModel;
using System.Text;

namespace sNet.CScriptPro;

public class Part : Obj
{
	private readonly List<Part> _children = [];
	
	public static readonly ReadOnlyTable Export = new Dictionary<Obj, Obj>()
	{
		{ "new", GlobalFunction.Create(New, TypeId.String) },
		{ "load", GlobalFunction.Create(Load, TypeId.String) },
	}.ToFrozenDictionary();

	public Part()
		: base(TypeId.Part) { }

	public virtual PartType PartType => PartType.Part;
	
	public IReadOnlyList<Part> Children => _children.AsReadOnly();

	private StrObj _name = StrObj.Empty;

	public StrObj Name
	{
		get => _name;
		set => ObserveSet(ref _name, value, "name");
	}

	private Bool _enabled = true;

	public Bool Enabled
	{
		get => _enabled;
		set => ObserveSet(ref _enabled, value, "enabled");
	}
	
	public Uid Uid { get; set; }
	
	public PartRoot Root { get; set; }
	
	public Part Parent { get; private set; }
	
	protected virtual string[] Properties => ["name", "enabled"];
	
	public override Obj this[Obj key]
	{
		get => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
		{
			"id" => PartType.ToString(),
			"name" => Name,
			"enabled" => Enabled,
			"parent" => (Obj)Parent ?? Nil.Value,
			"children" => new ArrayViewObj<Part>(_children),
			"addChild" => GlobalFunction.Create(args => AddChild((Part)args[0]), TypeId.Part),
			"removeChild" => GlobalFunction.Create(args => RemoveChild((Part)args[0]), TypeId.Part),
			"findFirstChild" => GlobalFunction.Create(args => FindFirstChild((StrObj)args[0]), TypeId.String),
			_ => FindFirstChild((StrObj)key),	
		};
		set
		{
			if (key.TypeId != TypeId.String) return;

			switch ((string)key)
			{
			case "name":
				Name = (string)value.Expect(TypeId.String);
				break;
			case "enabled":
				Enabled = (bool)value.Expect(TypeId.Bool);
				break;
			}
		}
	}

	public static Part Create(PartType partType) => partType switch
	{
		PartType.Part => new Part(),
		PartType.Part2d => new Part2d(),
		PartType.Script => new Script(),
		PartType.Camera2d => new Camera2d(),
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
		if (!_children.Remove(child))
		{
			return false;
		}

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

	public virtual int Serialize(Stream stream)
	{
		int written = stream.WriteNetByte((byte)PartType) + stream.WriteNetUtf8(Name) + stream.WriteBoolean(Enabled);

		written += stream.WriteNetInt32(_children.Count);
		
		foreach (var child in _children)
		{
			written += child.Serialize(stream);
		}
		
		return written;
	}

	public virtual void Deserialize(Stream stream)
	{
		Name = stream.ReadNetUtf8();
		Enabled = stream.ReadBoolean();

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

	protected virtual void ParentUpdate()
	{
	}

	protected bool ObserveSet<T>(ref T property, T value, string name)
		where T : Obj
	{
		if (EqualityComparer<T>.Default.Equals(property, value))
		{
			return false;
		}

		property = value;
		
		Root?.PropertyUpdate(this, name, value);

		return true;
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
			sb.AppendLine($"{property}: {this[property]},");
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