using SCENeo;
using SCENeo.Scenes;
using SCENeo.Ui;
using sNet;
using sNet.CScriptPro;

namespace sNetClient;

public sealed class LogScene : Scene
{
	private readonly UiConsole _console = new UiConsole()
	{
		Width = 60,
		Height = 12,
		BufferWidth = 60,
		BufferHeight = 2000,
		Layer = 10000,
		Anchor = Anchor.Right,
	};

	public override IEnumerable<IRenderable> Render()
	{
		return [_console];
	}

	public override void RawInput(ConsoleKeyInfo cki)
	{
		if (cki.Key != ConsoleKey.Oem8)
		{
			return;
		}
		
		Visible = !Visible;
	}

	public override void Start()
	{
		Enabled = true;
		Visible = false;

		Logger.Out = _console;
		IOPackage.Out = _console;
	}
}