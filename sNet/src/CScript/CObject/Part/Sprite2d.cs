using SCENeo;

namespace sNet.CScriptPro;

public sealed class Sprite2d : Render2d
{
	public StrObj _source;

	public StrObj Source
	{
		get => _source;
		set => ObserveSet(ref _source, value, "source");
	}

	protected override string[] Properties => [..base.Properties, "source"];

	public override Obj this[Obj key]
	{
		get => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
		{
			"source" => Source,
			_ => base[key],	
		};
		set
		{
			if (key.TypeId != TypeId.String) return;

			switch ((string)key)
			{
			case "source":
				Source = value.Expect<StrObj>(TypeId.String);
				break;
			default:
				base[key] = value;
				break;
			}
		}
	}

	public override IRenderable Render()
	{
		throw new NotImplementedException();
	}

	public override int Serialize(Stream stream)
	{
		return base.Serialize(stream) + stream.WriteNetUtf8(Source);
	}

	public override void Deserialize(Stream stream)
	{
		base.Deserialize(stream);
		Source = stream.ReadNetUtf8();
	}
}