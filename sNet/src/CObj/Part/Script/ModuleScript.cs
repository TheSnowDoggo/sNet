using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class ModuleScript : Part
{
	public new static readonly FrozenDictionary<string, IProperty> GlobalProperties = new Dictionary<string, IProperty>(Part.GlobalProperties)
	{
		{ "require", new GProperty<ModuleScript>(
			p => GlobalFunction.Create(p.Require))
		},
		{ "source", new GSProperty<ModuleScript, StrObj>(p => p.Source,
			(p, v) => p.Source = v, TypeId.String)
		},
	}.ToFrozenDictionary();

	private Obj _value;
	
	public StrObj Source { get; set; } = StrObj.Empty;

	public override PartType PartType => PartType.ModuleScript;

	public override IReadOnlyDictionary<string, IProperty> Properties => GlobalProperties;

	public Obj Require(Obj[] args)
	{
		if (_value != null)
		{
			return _value;
		}
		
		if (!ScriptLoader.Default.TryGet(Source, out var definition))
		{
			throw new InvalidOperationException("Failed to load module script.");
		}
		
		var context = new Context();
		
		context.CreateScope();
		context.Define("script", this);
		context.Define("root", (Obj)Root?.Root ?? Nil.Value);

		return _value = definition.Create(context).Run(args);
	}
}