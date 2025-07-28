using FFMpegCore.Pipes;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;

namespace PixiEditor.AnimationRenderer.FFmpeg;

public class ImgFrame : IVideoFrame, IDisposable
{
    public Image Image { get; }

    public int Width => Image.Width;
    public int Height => Image.Height;
    public string Format => ToStreamFormat(); 

    private Bitmap encoded;
    
    public ImgFrame(Image image)
    {
        Image = image;
        encoded = Bitmap.FromImage(image);
    }

    public void Serialize(Stream pipe)
    {
        var bytes = encoded.Bytes;
        pipe.Write(bytes, 0, bytes.Length);
    }

    public async Task SerializeAsync(Stream pipe, CancellationToken token)
    {
        await pipe.WriteAsync(encoded.Bytes, 0, encoded.Bytes.Length, token).ConfigureAwait(false); 
    }
    
    private string ToStreamFormat()
    {
        switch (encoded.Info.ColorType)
        {
            case ColorType.Gray8:
                return "gray8";
            case ColorType.Bgra8888:
                return "bgra";
            case ColorType.Rgba8888:
                return "rgba";
            case ColorType.Rgb565:
                return "rgb565";
            default:
                throw new NotSupportedException($"Color type {Image.Info.ColorType} is not supported.");
        } 
    }

    public void Dispose()
    {
        encoded.Dispose();
    }
}
