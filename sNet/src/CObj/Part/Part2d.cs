using System.Collections.Frozen;

namespace sNet.CScriptPro;

public class Part2d : Part
{
	protected new static readonly FrozenDictionary<string, IProperty> GlobalProperties = new Dictionary<string, IProperty>(Part.GlobalProperties)
	{
		{ "position", new GSProperty<Part2d, Vec2Obj>(p => p.Position,
			(p, v) => p.Position = v, TypeId.Vec2)
		},
		{ "globalPosition", new GSProperty<Part2d, Vec2Obj>(p => p.GlobalPosition,
			(p, v) => p.GlobalPosition = v, TypeId.Vec2, serializable: false)
		},
	}.ToFrozenDictionary();
	
	private Vec2Obj _position = Vector2.Zero;

	public Vec2Obj Position
	{
		get => _position;
		set
		{
			if (_position == value)
			{
				return;
			}
			
			_position = value;
			UpdateGlobalPosition();
		}
	}
	
	private Vec2Obj _globalPosition = Vector2.Zero;
	
	public Vec2Obj GlobalPosition
	{
		get => _globalPosition;
		set => Position = value - _globalPosition;
	}

	public override PartType PartType => PartType.Part2d;
	public override IReadOnlyDictionary<string, IProperty> Properties => GlobalProperties;

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