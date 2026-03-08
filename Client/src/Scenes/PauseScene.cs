using System.ComponentModel;
using SCENeo;
using SCENeo.Scenes;
using SCENeo.Serialization.BinImg;
using SCENeo.Ui;
using sNet;

namespace sNetClient;

public sealed class PauseScene : Scene
{
	private const ConsoleKey Toggle = ConsoleKey.Escape;

	private enum Option
	{
		Resume,
		Leave,
		Quit,
	}
	
	private static readonly string[] _options = Enum.GetNames<Option>();

	private readonly ListBoxItem _template = new ListBoxItem()
	{
		Anchor = Anchor.Center,
		FitToLength = true,
	};
	
	private readonly PlainView2D<Pixel> _view = new PlainView2D<Pixel>()
	{
		Value = Pixel.DarkGray,
	};
	
	private readonly ViewForwarder _vs;

	private readonly ListBox _listBox = new ListBox()
	{
		Width = 40,
		Height = _options.Length,
		Anchor = Anchor.Center | Anchor.Middle,
		Layer = 1003,
	};

	private DisplayMap _title = new DisplayMap();

	public PauseScene()
	{
		_vs = new ViewForwarder()
		{
			View = _view,
			Layer = 1000,
		};

		_listBox.Items = _template.FromTemplate(_options);
	}

	public override IEnumerable<IRenderable> Render()
	{
		return [_vs, _listBox, _title];
	}

	public override void Start()
	{
		Visible = false;

		try
		{
			_title = new DisplayMap(BinImgEncoding.DetectDeserialize("img/pause.binimg"))
			{
				Anchor = Anchor.Center,
				Offset = Vec2I.Down * 5,
				Layer = 1001,
			};
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to load pause title: {ex.Message}");
		}
	}

	public override void UnfocusedInput(ConsoleKeyInfo cki)
	{
		if (cki.Key != Toggle || Visible)
		{
			return;
		}
		
		Visible = true;
		Parent.InputFocus = this;
	}

	public override void FocusedInput(ConsoleKeyInfo cki)
	{
		switch (cki.Key)
		{
		case Toggle:
			Hide();
			break;
		case ConsoleKey.UpArrow:
			_listBox.LimitMove(-1);
			break;
		case ConsoleKey.DownArrow:
			_listBox.LimitMove(+1);
			break;
		case ConsoleKey.Enter:
			Select();
			break;
		}
	}

	public override void DisplayResize(Vec2I size)
	{
		_view.Width = size.X;
		_view.Height = size.Y;
	}

	private void Hide()
	{
		Visible = false;
		Parent.InputFocus = null;
	}

	private void Select()
	{
		var option = (Option)_listBox.Selected;

		switch (option)
		{
		case Option.Resume:
			Hide();
			break;
		case Option.Leave:
			Hide();
			Client.Instance.Net.DisconnectAsync();
			break;
		case Option.Quit:
			Hide();
			Client.Instance.Net.DisconnectAsync();
			Client.Instance.Updater.Stop();
			break;
		default:
			throw new InvalidEnumArgumentException(nameof(option), (int)option, typeof(Option));
		}
	}
}