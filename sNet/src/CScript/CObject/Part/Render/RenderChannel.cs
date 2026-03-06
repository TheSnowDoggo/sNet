using SCENeo;
using SCENeo.Ui;

namespace sNet.CScriptPro;

public sealed class RenderChannel : UiBaseDimensioned, IRenderable
{
	private readonly Image _buffer = [];

	private Pixel _basePixel;

	public Pixel BasePixel
	{
		get => _basePixel;
		set => ObserveSet(ref _basePixel, value);
	}

	public void Draw(IView<Pixel> view, Vec2I screenOffset)
	{
		_buffer.MergeMap(view, screenOffset);
	}
	
	public void Clear()
	{
		_buffer.TryCleanResize(Width, Height);
		_buffer.Fill(BasePixel);
	}
	
	public IView<Pixel> Render()
	{
		if (!_update)
		{
			return _buffer;
		}

		_buffer.TryCleanResize(Width, Height);
		
		return _buffer;
	}
}