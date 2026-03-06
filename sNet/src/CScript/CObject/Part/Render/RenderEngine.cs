using SCENeo;

namespace sNet.CScriptPro;

public sealed class RenderEngine
{
	private sealed class OutputInfo
	{
		public RenderChannel Channel { get; init; }
		public Vector2 Position { get; init; }
	}

	private sealed class InputInfo : IComparable<InputInfo>
	{
		public IRenderable Renderable { get; init; }
		public Vector2 Position { get; init; }
		public double Layer { get; init; }
		public Anchor Anchor { get; init; }

		public int CompareTo(InputInfo other)
		{
			return other is not null ? Layer.CompareTo(other.Layer) : -1;
		}
	}

	private HashSet<int> _seenChannels;
	private Queue<OutputInfo> _outputs;
	private List<InputInfo> _inputs;
	
	public Dictionary<int, RenderChannel> Channels { get; init; } = [];
	
	public Part Root { get; set; }
	
	public void Render()
	{
		if (Root == null)
		{
			throw new NullReferenceException("Root was not assigned.");
		}

		if (!Root.Enabled || !Root.Visible)
		{
			return;
		}

		_seenChannels = [];
		_outputs = [];
		_inputs = [];

		try
		{
			ProcessParts();
			Draw();
		}
		finally
		{
			_seenChannels = null;
			_outputs = null;
			_inputs = null;
		}
	}

	private void Draw()
	{
		_inputs.Sort();
		
		var views = new IView<Pixel>[_inputs.Count];

		while (_outputs.TryDequeue(out var outInfo))
		{
			var channel = outInfo.Channel;
			
			channel.Clear();

			var screenArea = new Rect2D(channel.Width, channel.Height);
			
			for (int i = 0; i < _inputs.Count; i++)
			{
				var inInfo = _inputs[i];

				var size = inInfo.Renderable.Size();
				
				Vec2I screenOffset = (Vec2I)(inInfo.Position.Rounded() - outInfo.Position.Rounded());
				screenOffset += inInfo.Renderable.Offset + inInfo.Anchor.AnchorDimension(size) - size;
				
				Vec2I screenEnd = screenOffset + screenOffset;

				if (!screenArea.Overlaps(screenOffset, screenEnd))
				{
					continue;
				}

				var view = views[i] ??= inInfo.Renderable.Render();
				
				channel.Draw(view, screenOffset);
			}
		}
	}

	private void ProcessParts()
	{
		var queue = new Queue<Part>();
		queue.Enqueue(Root);

		while (queue.TryDequeue(out var part))
		{
			ProcessPart(part);
			
			foreach (var child in part.Children)
			{
				if (!part.Enabled || !part.Visible)
				{
					continue;
				}
				
				queue.Enqueue(child);
			}
		}
	}

	private void ProcessPart(Part part)
	{
		switch (part)
		{
		case Camera2d camera2d:
			ProcessCamera(camera2d);
			break;
		case Render2d render2d:
			ProcessRender(render2d);
			break;
		}
	}

	private void ProcessCamera(Camera2d camera2d)
	{
		int channelId = (int)camera2d.Channel;

		if (!_seenChannels.Add(channelId))
		{
			return;
		}

		if (!Channels.TryGetValue(channelId, out var channel))
		{
			Logger.Error($"Camera {camera2d.Name} references unknown channel {channelId}.");
			return;
		}

		var output = new OutputInfo()
		{
			Channel = channel,
			Position = camera2d.GlobalPosition,
		};
		
		_outputs.Enqueue(output);
	}

	private void ProcessRender(Render2d render2d)
	{
		if (!TryParseAnchor(render2d.Anchor, out var anchor))
		{
			Logger.Error($"Invalid anchor {render2d.Anchor}.");
			return;
		}

		var renderable = render2d.Render();

		if (renderable == null)
		{
			return;
		}

		var input = new InputInfo()
		{
			Renderable = renderable,
			Position = render2d.GlobalPosition,
			Layer = render2d.Layer,
			Anchor = anchor,
		};
		
		_inputs.Add(input);
	}

	private static bool TryParseAnchor(string anchorStr, out Anchor anchor)
	{
		anchor = anchorStr.ToLower() switch
		{
			"" or "tl" => Anchor.None,
			"tc" => Anchor.Center,
			"tr" => Anchor.Right,
			"bl" => Anchor.Bottom,
			"bc" => Anchor.Bottom | Anchor.Center,
			"br" => Anchor.Bottom | Anchor.Right,
			"cl" => Anchor.Center,
			"cm" => Anchor.Center | Anchor.Middle,
			"cr" => Anchor.Center | Anchor.Right,
			_ => (Anchor)(-1),
		};

		return anchor != (Anchor)(-1);
	}
}