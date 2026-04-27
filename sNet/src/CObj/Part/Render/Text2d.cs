using System.Collections.Frozen;
using SCENeo;
using SCENeo.Ui;

namespace sNet.CScriptPro;

public sealed class Text2d : Render2d
{
	public new static readonly FrozenDictionary<string, IProperty> GlobalProperties = new Dictionary<string, IProperty>(Render2d.GlobalProperties)
	{
		{ "width", new GSProperty<Text2d, Number>(p => p._label.Width,
			(p, v) => p._label.Width = int.Max((int)v, 0), TypeId.Number)
		},
		{ "height", new GSProperty<Text2d, Number>(p => p._label.Height,
			(p, v) => p._label.Height = int.Max((int)v, 0), TypeId.Number)
		},
		{ "fgColor", new GSProperty<Text2d, Number>(p => (int)p._label.TextFgColor,
			(p, v) => p._label.TextFgColor = (SCEColor)v, TypeId.Number)
		},
		{ "bgColor", new GSProperty<Text2d, Number>(p => (int)p._label.TextBgColor,
			(p, v) =>p._label.TextBgColor = (SCEColor)v,TypeId.Number)
		},
		{ "color", new GSProperty<Text2d, Number>(p => (int)p._label.BasePixel.BgColor,
			(p, v) => p._label.BasePixel = new Pixel((SCEColor)v),TypeId.Number)
		},
		{ "textAnchor", new GSProperty<Text2d, StrObj>(p => p._textAnchor,
			(p, v) => p._textAnchor = v, TypeId.String)
		},
		{ "text", new GSProperty<Text2d, StrObj>(p => p._label.Text,
			(p, v) => p._label.Text = v, TypeId.String)
		},
		{ "fitToLength", new GSProperty<Text2d, Bool>(p => p._label.FitToLength,
			(p, v) => p._label.FitToLength = v, TypeId.Bool)
		},
	}.ToFrozenDictionary();
	
	private readonly TextLabel _label = new TextLabel();

	private StrObj _textAnchor = StrObj.Empty;

	public override PartType PartType => PartType.Text2d;

	public override IReadOnlyDictionary<string, IProperty> Properties => GlobalProperties;

	public override IRenderable Render()
	{
		_label.Anchor = RenderEngine.ParseAnchor(_textAnchor);
		
		return _label;
	}
}