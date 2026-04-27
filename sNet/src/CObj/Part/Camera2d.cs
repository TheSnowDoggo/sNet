using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class Camera2d : Part2d
{
	public new static readonly FrozenDictionary<string, IProperty> GlobalProperties = new Dictionary<string, IProperty>(Part2d.GlobalProperties)
	{
		{ "channel", new GSProperty<Camera2d, Number>(p => p.Channel,
			(p, v) => p.Channel = v, TypeId.Number)
		},
	}.ToFrozenDictionary();

	public Number Channel { get; set; } = 0;

	public override PartType PartType => PartType.Camera2d;

	public override IReadOnlyDictionary<string, IProperty> Properties => GlobalProperties;
}