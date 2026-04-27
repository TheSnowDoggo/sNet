using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class Script : Part
{
	public new static readonly FrozenDictionary<string, IProperty> GlobalProperties = new Dictionary<string, IProperty>(Part.GlobalProperties)
	{
		{ "source", new GSProperty<Script, StrObj>(p => p.Source,
			(p, v) => p.Source = v, TypeId.String)
		},
		{ "type", new GSProperty<Script, StrObj>(p => p.Type,
			(p, v) => p.Type = v, TypeId.String)
		},
	}.ToFrozenDictionary();
	
	public StrObj Source { get; set; } = StrObj.Empty;
	public StrObj Type { get; set; } = "server";
	
	public override PartType PartType => PartType.Script;

	public override IReadOnlyDictionary<string, IProperty> Properties => GlobalProperties;

	public ScriptFlag Flags => Type.ToString().ToLower() switch
	{
		"server" => ScriptFlag.Server,
		"client" => ScriptFlag.Client,
		"both" => ScriptFlag.Server | ScriptFlag.Client,
		_ => ScriptFlag.Server,
	};
	
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