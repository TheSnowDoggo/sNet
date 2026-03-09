using System.ComponentModel;
using SCENeo;
using SCENeo.Input;
using SCENeo.Scenes;
using SCENeo.Serialization.BinImg;
using SCENeo.Ui;
using sNet;

namespace sNetClient;

public sealed class ConnectScene : Scene
{
	private const int Fields  = 2;
	private const int Options = 3;

	private enum Option
	{
		Hostname,
		Port,
		Connect,
		Credits,
		Quit,
	}

	private static readonly string[] _fieldText =
	[
		"Hostname : ",
		"Port     : ",
	];
	
	private static readonly string[] _defaultFieldValues =
	[
		"192.168.0.157",
		"17324",
	];
	
	private static readonly string[] _optionText =
	[
		"Connect",
		"Credits",
		"Quit",
	];

	private static readonly ListBoxItem _optionTemplate = new ListBoxItem()
	{
		FitToLength = true,
		SelectedBgColor = SCEColor.Magenta,
		UnselectedBgColor = SCEColor.DarkMagenta,
		Anchor = Anchor.Center,
	};
	
	private readonly ListBoxItem _fieldTemplate = new ListBoxItem()
	{
		FitToLength = true,
		SelectedBgColor = SCEColor.White,
	};

	private DisplayMap _title = new DisplayMap();

	private readonly TextLabel _credits = new TextLabel()
	{
		Width = 40,
		Height = 3,
		Anchor = Anchor.Right,
		Text = "Author : Made by Luna Sparkle.\n" +
		       "Ui     : SCENeo 1.5.0\n" +
		       "sNet   : sNet 0.0.0",
		TextBgColor = SCEColor.Transparent,
		BasePixel = Pixel.DarkGreen,
		Offset = Vec2I.Down,
		Visible = false,
	};
	
	private readonly ListBox _listBox = new ListBox()
	{
		Width = 50,
		Height = Fields + Options,
		Anchor = Anchor.Center | Anchor.Middle,
	};

	private readonly ProgressBar _progressBar = new ProgressBar()
	{
		Width = 80,
		Height = 16,
		Max = 1,
		Min = -1,
		Anchor = Anchor.Center | Anchor.Middle,
		Layer = -100,
		Mode = ProgressBarFlow.TopBottom,
		BackPixel = Pixel.DarkYellow,
		FillPixel = Pixel.Cyan,
	};

	private readonly Cursor _cursor = new Cursor();

	private readonly VirtualOverlay _overlay;
	
	private readonly TextEntry[] _textEntries = new TextEntry[Fields];

	private bool _typing;

	private double _time;
	
	public ConnectScene()
	{
		_overlay = new VirtualOverlay()
		{
			Source = _listBox,
			Overlay = _cursor,
		};

		var items = new UpdateList<ListBoxItem>(Fields + Options);

		for (int i = 0; i < Fields; i++)
		{
			items.Add(_fieldTemplate.FromTemplate(_defaultFieldValues[i]));
		}

		for (int i = 0; i < Options; i++)
		{
			items.Add(_optionTemplate.FromTemplate(_optionText[i]));
		}
		
		_listBox.Items = items;
		
		for (int i = 0; i < _textEntries.Length; i++)
		{
			var text = _defaultFieldValues[i] ?? string.Empty;
			
			var entry = new TextEntry
			{
				Text = text,
				Index = text.Length,
			};

			entry.TextChanged += TextEntry_OnTextChanged;
			entry.IndexChanged += TextEntry_OnIndexChanged;
			
			_textEntries[i] = entry;
			
			UpdateField(i);
		}
	}

	public override IEnumerable<IRenderable> Render()
	{
		return [_overlay, _title, _credits, _progressBar];
	}

	public override void Open()
	{
		base.Open();
		Parent.InputFocus = this;
		Client.Instance.Viewport.BasePixel = Pixel.DarkCyan;
	}

	public override void Close()
	{
		base.Close();
		Parent.InputFocus = null;
	}

	public override void Start()
	{
		try
		{
			var img = BinImgEncoding.DetectDeserialize("img/snet.binimg");

			_title = new DisplayMap(img)
			{
				Anchor = Anchor.Center,
				Offset = Vec2I.Down * 5,
				Layer = -10,
			};
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to load title: {ex.Message}");
		}
		
		Open();
	}

	public override void Update(double delta)
	{
		_progressBar.Value = Math.Sin(_time);
		_time += delta;
		
		if (_typing)
		{
			_cursor.Update(delta);
		}
	}

	public override void FocusedInput(ConsoleKeyInfo cki)
	{
		if (_typing)
		{
			TypingInput(cki);
		}
		else
		{
			SelectingInput(cki);
		}
	}

	private void TypingInput(ConsoleKeyInfo cki)
	{
		switch (cki.Key)
		{
		case ConsoleKey.Escape:
		case ConsoleKey.Enter:
			StopTyping();
			break;
		default:
			_textEntries[_listBox.Selected].Next(cki);
			break;
		}
	}

	private void SelectingInput(ConsoleKeyInfo cki)
	{
		switch (cki.Key)
		{
		case ConsoleKey.UpArrow:
			_listBox.LimitMove(-1);
			break;
		case ConsoleKey.DownArrow:
			_listBox.LimitMove(1);
			break;
		case ConsoleKey.Enter:
			if (_listBox.Selected < Fields)
			{
				StartTyping();
				break;
			}
			OptionSelect((Option)_listBox.Selected);
			break;
		}
	}

	private void OptionSelect(Option option)
	{
		switch (option)
		{
		case Option.Connect:
			StartConnect();
			break;
		case Option.Credits:
			_credits.Visible = !_credits.Visible;
			break;
		case Option.Quit:
			Client.Instance.Updater.Stop();
			break;
		default:
			throw new InvalidEnumArgumentException(nameof(option), (int)option, typeof(Option));
		}
	}

	private void StartConnect()
	{
		string hostName = _textEntries[(int)Option.Hostname].Text;

		if (string.IsNullOrWhiteSpace(hostName))
		{
			Client.Instance.Alert.Alert("Host name is empty.");
			return;
		}
		
		string portStr = _textEntries[(int)Option.Port].Text;

		if (string.IsNullOrWhiteSpace(portStr))
		{
			Client.Instance.Alert.Alert("Port is empty.");
			return;
		}
		
		if (!int.TryParse(portStr, out int port) || port is < 0 or > 65535)
		{
			Client.Instance.Alert.Alert($"Port \'{portStr}\' is invalid.");
			return;
		}

		var load = Client.Instance.Loading;
		
		ChangeTo(load);
		Task.Run(() => load.Connect(hostName, port));
	}

	private void StartTyping()
	{
		_typing = true;
		TextEntry_OnIndexChanged();
		_fieldTemplate.SelectedBgColor = SCEColor.Gray;
	}

	private void StopTyping()
	{
		_typing = false;
		_cursor.Visible = false;
		_fieldTemplate.SelectedBgColor = SCEColor.White;
	}

	private void TextEntry_OnTextChanged()
	{
		UpdateField(_listBox.Selected);
		_cursor.HoldCursor();
	}
	
	private void TextEntry_OnIndexChanged()
	{
		int x = _fieldText[_listBox.Selected].Length + _textEntries[_listBox.Selected].Index;
		_cursor.Offset = new Vec2I(x, _listBox.Selected);
		_cursor.HoldCursor();
	}

	private void UpdateField(int field)
	{
		_listBox.Items[field].Text = _fieldText[field] + _textEntries[field].Text;
	}
}