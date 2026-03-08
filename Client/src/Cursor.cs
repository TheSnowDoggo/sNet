using SCENeo;
using SCENeo.Ui;

namespace sNetClient;

public sealed class Cursor : UiBase, IRenderable
{
	private readonly Image _buffer = new Image(1, 1)
	{
		[0, 0] = new Pixel('\0', SCEColor.Black, SCEColor.White),
	};
	
	private double _blinkTimer = 0;

	public double BlinkInterval { get; set; } = 0.5;
	
	public int Width => 1;
	public int Height => 1;

	public Cursor()
	{
		Visible = false;
	}

	public void Update(double delta)
	{
		_blinkTimer += delta;

		if (_blinkTimer < BlinkInterval)
		{
			return;
		}

		Visible = !Visible;
		_blinkTimer = 0;
	}

	public void HoldCursor()
	{
		_blinkTimer = 0;
		Visible = true;
	}

	public IView<Pixel> Render()
	{
		return _buffer;
	}
}