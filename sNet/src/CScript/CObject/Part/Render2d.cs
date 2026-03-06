namespace sNet.CScriptPro;

public abstract class Render2d : Part2d
{
	private Number _channel;
	
	public Number Channel
	{
		get => _channel;
		set => ObserveSet(ref _channel, value, "channel");
	}

	protected override string[] Properties => [..base.Properties, "channel"];

	public override Obj this[Obj key]
	{
		get => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
		{
			"channel" => Channel,
			_ => base[key],
		};
		set
		{
			if (key.TypeId != TypeId.String) return;

			switch ((string)key)
			{
			case "channel":
				Channel = (Number)value.Expect(TypeId.Number);
				break;
			default:
				base[key] = value;
				break;
			}
		}
	}

	public override int Serialize(Stream stream)
	{
		return base.Serialize(stream) + stream.WriteNetDouble(Channel);
	}

	public override void Deserialize(Stream stream)
	{
		Channel = stream.ReadNetInt64();
	}
}