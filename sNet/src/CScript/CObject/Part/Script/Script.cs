namespace sNet.CScriptPro;

public sealed class Script : Part
{
	private StrObj _source = StrObj.Empty;

	public StrObj Source
	{
		get => _source;
		set => ObserveSet(ref _source, value, "source");
	}

	private StrObj _type = "server";

	public StrObj Type
	{
		get => _type;
		set => ObserveSet(ref _type, value, "type");
	}
	
	public override PartType PartType => PartType.Script;

	protected override string[] Properties => [..base.Properties, "source", "type"];
	
	public ScriptFlag Flags => _type.ToString().ToLower() switch
	{
		"server" => ScriptFlag.Server,
		"client" => ScriptFlag.Client,
		"both" => ScriptFlag.Server | ScriptFlag.Client,
		_ => ScriptFlag.Server,
	};
	
	public override Obj this[Obj key]
	{
		get => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
		{
			"source" => Source,
			"type" => Type,
			_ => base[key],
		};
		set
		{
			if (key.TypeId != TypeId.String) return;

			switch ((string)key)
			{
			case "source":
				Source = (StrObj)value.Expect(TypeId.String);
				break;
			case "type":
				Type = (StrObj)value.Expect(TypeId.String);
				break;
			default:
				base[key] = value;
				break;
			}
		}
	}
	
	public static void RunScripts(Part root, ScriptFlag flags)
	{
		if (!root.Enabled)
		{
			return;
		}
		
		var queue = new Queue<Part>();
		queue.Enqueue(root);

		while (queue.TryDequeue(out var part))
		{
			if (part is Script script)
			{
				try
				{
					script.Run(flags);
				}
				catch (Exception ex)
				{
					Logger.Error($"While running script {script.Name}:{script.Source}: {ex.Message}");
				}
			}
			
			foreach (var child in part.Children)
			{
				if (!child.Enabled)
				{
					continue;
				}
				
				queue.Enqueue(child);
			}
		}
	}

	public override int Serialize(Stream stream)
	{
		return base.Serialize(stream) 
		       + stream.WriteNetUtf8(Source)
		       + stream.WriteNetUtf8(Type);
	}

	public override void Deserialize(Stream stream)
	{
		base.Deserialize(stream);
		Source = stream.ReadNetUtf8();
		Type = stream.ReadNetUtf8();
	}

	public bool Run(ScriptFlag source)
	{
		if ((source & Flags) == 0)
		{
			return false;
		}
		
		if (!ScriptLoader.Default.TryGet(Source, out var definition))
		{
			return false;
		}

		var context = new Context();
		
		context.CreateScope();
		context.Define("script", this);
		context.Define("root", (Obj)Root?.Root ?? Nil.Value);

		definition.Create(context).Run();

		return true;
	}
}