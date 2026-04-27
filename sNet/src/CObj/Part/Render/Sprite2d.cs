using System.Collections.Frozen;
using SCENeo;
using SCENeo.Ui;

namespace sNet.CScriptPro;

public sealed class Sprite2d : Render2d
{
	public new static readonly FrozenDictionary<string, IProperty> GlobalProperties = new Dictionary<string, IProperty>(Render2d.GlobalProperties)
	{
		{ "source", new GSProperty<Sprite2d, StrObj>(p => p.Source,
			(p, v) => p.Source = v, TypeId.String)
		},
	}.ToFrozenDictionary();
	
	private DisplayMap _data;

	public StrObj Source { get; set; } = StrObj.Empty;

	public override PartType PartType => PartType.Sprite2d;

	public override IReadOnlyDictionary<string, IProperty> Properties => GlobalProperties;

	public override IRenderable Render()
	{
		if (_data != null)
		{
			return _data;
		}

		if (!ImageLoader.Default.TryGet(Source, out var image))
		{
			return null;
		}
		
		return _data = new DisplayMap(image);
	}
}