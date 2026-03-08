using SCENeo;
using SCENeo.Scenes;
using SCENeo.Ui;

namespace sNetClient;

public sealed class AlertScene : Scene
{
	private sealed record AlertInfo(string Message, double Duration, SCEColor Color);
	
	private readonly TextLabel _alertBox = new TextLabel()
	{
		Width = 60,
		Height = 3,
		TextFgColor = SCEColor.White,
		TextBgColor = SCEColor.Transparent,
		TextAnchor = Anchor.Center | Anchor.Middle,
		TextWrapping = TextWrapping.Word,
		Visible = false,
		Anchor = Anchor.Right | Anchor.Bottom,
	};

	private readonly Queue<AlertInfo> _alerts = [];

	private double _alertTimer = 0;

	public override void Start()
	{
		Enabled = true;
	}

	public override IEnumerable<IRenderable> Render()
	{
		return [_alertBox];
	}

	public override void Update(double delta)
	{
		_alertTimer -= delta;

		if (_alertTimer > 0)
		{
			return;
		}
		
		if (!_alerts.TryDequeue(out AlertInfo info))
		{
			_alertBox.Visible = false;
			return;
		}
		
		_alertBox.Text = info.Message;
		_alertBox.BasePixel = new Pixel(info.Color);
		_alertBox.Visible = true;

		_alertTimer = info.Duration;
	}

	public void Alert(string message, double duration = 1, SCEColor color = SCEColor.Red)
	{
		_alerts.Enqueue(new AlertInfo(message, duration, color));
	}
}