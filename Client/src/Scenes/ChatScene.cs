using SCENeo;
using SCENeo.Input;
using SCENeo.Scenes;
using SCENeo.Ui;
using sNet;
using sNet.Service.Chat;
using sNet.Service.Cmd;

namespace sNetClient;

public sealed class ChatScene : Scene
{
	private const ConsoleKey ChatKey = ConsoleKey.Enter;

	private readonly TextEntry _textEntry = new TextEntry();

	private readonly UiConsole _chat = new UiConsole()
	{
		Width = 60,
		Height = 10,
		BufferWidth = 60,
		BufferHeight = 200,
		BasePixel = Pixel.Null,
		ForegroundColor = SCEColor.White,
		BackgroundColor = SCEColor.Transparent,
	};

	private readonly TextLabel _entryLabel = new TextLabel()
	{
		Width = 60,
		Height = 1,
		Offset = Vec2I.Down * 10,
		Visible = false,
		BasePixel = Pixel.DarkGray,
		TextBgColor = SCEColor.Transparent,
	};

	private readonly Cursor _cursor = new Cursor();

	private readonly VirtualOverlay _entryOverlay;

	private ClientChatService _chatService;
	private ClientCmdService _cmdService;

	public ChatScene()
	{
		_entryOverlay = new VirtualOverlay()
		{
			Source = _entryLabel,
			Overlay = _cursor,
		};
	}

	public override IEnumerable<IRenderable> Render()
	{
		return [_chat, _entryOverlay];
	}

	public override void Start()
	{
		_chatService = Client.Instance.Net.Services.Get<ClientChatService>(ServiceId.Chat);
		_chatService.ChatReceived += message => _chat.WriteLine(message);
		
		_cmdService = Client.Instance.Net.Services.Get<ClientCmdService>(ServiceId.Cmd);
		_cmdService.ResponseReceived += response => _chat.WriteLine(response);
		
		_textEntry.TextChanged += TextEntry_OnTextChanged;
		_textEntry.IndexChanged += TextEntry_OnIndexChanged;
		_textEntry.Enter += TextEntry_OnEnter;
	}

	public override void Update(double delta)
	{
		if (Focused)
		{
			_cursor.Update(delta);
		}
	}

	public override void UnfocusedInput(ConsoleKeyInfo cki)
	{
		if (cki.Key != ChatKey)
		{
			return;
		}
		
		Parent.InputFocus = this;
		_entryLabel.Visible = true;
	}

	public override void FocusedInput(ConsoleKeyInfo cki)
	{
		ChattingInput(cki);
	}
	
	private void ChattingInput(ConsoleKeyInfo cki)
	{
		switch (cki.Key)
		{
		case ConsoleKey.UpArrow:
			if (_chat.Scroll.Y > 0)
			{
				_chat.Scroll += Vec2I.Up;
			}
			break;
		case ConsoleKey.DownArrow:
			_chat.Scroll += Vec2I.Down;
			break;
		default:
			_textEntry.Next(cki);
			break;
		}
	}
	
	private void TextEntry_OnTextChanged()
	{
		_entryLabel.Text = _textEntry.Text;
		_cursor.HoldCursor();
	}
	
	private void TextEntry_OnIndexChanged()
	{
		_cursor.Offset = new Vec2I(_textEntry.Index, 0);
		_cursor.HoldCursor();
	}
	
	private void TextEntry_OnEnter()
	{
		string text = _textEntry.Text;
		_textEntry.Clear();

		Parent.InputFocus = null;
		_entryLabel.Visible = false;
		
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		
		if (text.Length > 256)
		{
			Client.Instance.Alert.Alert("Message is too long.");
			return;
		}
		
		if (text[0] == '/')
		{
			_chat.WriteLine($"<cmd> {text}");
			_cmdService.FireCmd(text[1..]);
		}
		else
		{
			_chat.WriteLine($"<you> {text}");
			_chatService.FireChat(text);
		}
	}
}