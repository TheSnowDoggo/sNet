using SCENeo;
using SCENeo.Serialization;
using SCENeo.Serialization.BinImg;

namespace sNet;

public sealed class ImageLoader
{
	private readonly Dictionary<string, Image> _images = [];
	
	public static ImageLoader Default { get; } = new ImageLoader();

	public bool TryGet(string source, out Image image)
	{
		if (_images.TryGetValue(source, out image))
		{
			return true;
		}

		try
		{
			image = source.StartsWith(SIFSerializer.Signature)
				? new Image(SIFSerializer.DeserializeString(source))
				: new Image(BinImgEncoding.DetectDeserialize(source));

			_images[source] = image;
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to load image at {source}: {ex.Message}");
			return false;
		}
	}
}