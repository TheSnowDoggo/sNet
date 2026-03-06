namespace sNet.CScriptPro;

public class Part2d : Part
{
	private CVec2 _position = Vec2.Zero;

	public CVec2 Position
	{
		get => _position;
		set
		{
			if (ObserveSet(ref _position, value, "position"))
			{
				UpdateGlobalPosition();
			}
		}
	}
	
	private CVec2 _globalPosition = Vec2.Zero;
	
	public CVec2 GlobalPosition
	{
		get => _globalPosition;
		set => Position = value - _globalPosition;
	}

	public override PartType PartType => PartType.Part2d;

	protected override string[] Properties => [..base.Properties, "position", "globalPosition"];

	public override CObj this[CObj key]
	{
		get => key.TypeId != TypeId.String ? Nil.Value : (string)key switch
		{
			"position" => Position,
			"globalPosition" => GlobalPosition,
			_ => base[key],	
		};
		set
		{
			if (key.TypeId != TypeId.String) return;

			switch ((string)key)
			{
			case "position":
				Position = (CVec2)value.Expect(TypeId.Vec2);
				break;
			case "globalPosition":
				GlobalPosition = (CVec2)value.Expect(TypeId.Vec2);
				break;
			default:
				base[key] = value;
				break;
			}
		}
	}

	public override int Serialize(Stream stream)
	{
		return base.Serialize(stream) + stream.WriteVec2(Position);
	}

	public override void Deserialize(Stream stream)
	{
		base.Deserialize(stream);
		Position = stream.ReadVec2();
	}

	protected override void ParentUpdate()
	{
		UpdateGlobalPosition();
	}

	private void UpdateGlobalPosition()
	{
		_globalPosition = Parent is Part2d parent ? parent._globalPosition + _position : _position;

		foreach (var child in Children)
		{
			if (child is Part2d part2d)
			{
				part2d.UpdateGlobalPosition();
			}
		}
	}
}