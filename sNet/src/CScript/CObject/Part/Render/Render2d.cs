using System.Collections.Frozen;
using SCENeo;

namespace sNet.CScriptPro;

public abstract class Render2d : Part2d
{
	public new static readonly FrozenDictionary<string, IProperty> GlobalProperties = new Dictionary<string, IProperty>(Part2d.GlobalProperties)
	{
		{ "layer", new GSProperty<Render2d, Number>(p => p.Layer, (p, v) => p.Layer = v, TypeId.Number) },
		{ "anchor", new GSProperty<Render2d, StrObj>(p => p.Anchor, (p, v) => p.Anchor = v, TypeId.String) },
	}.ToFrozenDictionary();
	
	public Number Layer { get; set; } = 0;
	public StrObj Anchor { get; set; } = StrObj.Empty;

	public override IReadOnlyDictionary<string, IProperty> Properties => GlobalProperties;

	public abstract IRenderable Render();
}