using SCENeo;

namespace sNet.CScriptPro;

public abstract class Render2d : Part2d
{
	private Number _layer = 0;
	
	public Number Layer
	{
		get => _layer;
		set => ObserveSet(ref _layer, value, "layer");
	}

	private StrObj _anchor = StrObj.Empty;

	public StrObj Anchor
	{
		get => _anchor;
		set => ObserveSet(ref _anchor, value, "anchor");
	}
	
	protected override string[] Properties => [..base.Properties, "layer"];

	public override Obj this[Obj key]
	{
		get => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
		{
			"layer" => Layer,
			"anchor" => Anchor,
			_ => base[key],
		};
		set
		{
			if (key.TypeId != TypeId.String) return;

			switch ((string)key)
			{
			case "channel":
				Layer = value.Expect<Number>(TypeId.Number);
				break;
			case "anchor":
				Anchor = value.Expect<StrObj>(TypeId.String);
				break;
			default:
				base[key] = value;
				break;
			}
		}
	}

	public abstract IRenderable Render();

	public override int Serialize(Stream stream)
	{
		return base.Serialize(stream) + stream.WriteNetDouble(Layer) + stream.WriteNetUtf8(Anchor);
	}

	public override void Deserialize(Stream stream)
	{
		base.Deserialize(stream);
		Layer = stream.ReadNetDouble();
		Anchor = stream.ReadNetUtf8();
	}
}