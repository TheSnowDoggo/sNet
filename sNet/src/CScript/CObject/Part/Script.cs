namespace sNet.CScriptPro;

public sealed class Script : Part
{
	private StrObj _source = StrObj.Empty;

	public StrObj Source
	{
		get => _source;
		set => ObserveSet(ref _source, value, "source");
	}
	
	public override PartType PartType => PartType.Script;

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
				Source = (StrObj)value.Expect(TypeId.String);
				break;
			default:
				base[key] = value;
				break;
			}
		}
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