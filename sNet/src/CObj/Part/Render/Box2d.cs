using System.Collections.Frozen;
using SCENeo;
using SCENeo.Ui;

namespace sNet.CScriptPro;

public sealed class Box2d : Render2d
{
	public new static readonly FrozenDictionary<string, IProperty> GlobalProperties = new Dictionary<string, IProperty>(Render2d.GlobalProperties)
	{
		{ "width", new GSProperty<Box2d, Number>(p => p._view.Width,
			(p, v) => p._view.Width = int.Max((int)v, 0), TypeId.Number)
		},
		{ "height", new GSProperty<Box2d, Number>(p => p._view.Height,
			(p, v) => p._view.Height = int.Max((int)v, 0), TypeId.Number)
		},
		{ "color", new GSProperty<Box2d, Number>(p => (int)p._view.Value.BgColor,
			(p, v) => p._view.Value = new Pixel((SCEColor)v),TypeId.Number)
		},
	}.ToFrozenDictionary();
	
	private readonly PlainView2D<Pixel> _view = new PlainView2D<Pixel>();

	private readonly ViewForwarder _vf;

	public Box2d()
	{
		_vf = new ViewForwarder()
		{
			View = _view,
		};
	}

	public override PartType PartType => PartType.Box2d;

	public override IReadOnlyDictionary<string, IProperty> Properties => GlobalProperties;

	public override IRenderable Render() => _vf;
}