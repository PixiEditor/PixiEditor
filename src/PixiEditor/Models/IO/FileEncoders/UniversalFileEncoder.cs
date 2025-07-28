using System.IO;
using System.Threading.Tasks;
using ChunkyImageLib;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;

namespace PixiEditor.Models.IO.FileEncoders;

public class UniversalFileEncoder : IFileEncoder
{
    public bool SupportsTransparency { get; }
    public EncodedImageFormat Format { get; set; }

    //TODO: Check if this is correct and exclude unsupported formats
    public UniversalFileEncoder(EncodedImageFormat format)
    {
        Format = format;
        SupportsTransparency = FormatSupportsTransparency(format);
    }

    public async Task SaveAsync(Stream stream, Surface bitmap)
    {
        await bitmap.DrawingSurface.Snapshot().Encode(Format).AsStream().CopyToAsync(stream);
    }

    public void Save(Stream stream, Surface bitmap)
    {
        bitmap.DrawingSurface.Snapshot().Encode(Format).AsStream().CopyTo(stream);
    }

    private bool FormatSupportsTransparency(EncodedImageFormat format)
    {
        switch (format)
        {
            case EncodedImageFormat.Bmp:
                return false;
            case EncodedImageFormat.Gif:
                return true;
            case EncodedImageFormat.Ico:
                return true;
            case EncodedImageFormat.Jpeg:
                return false;
            case EncodedImageFormat.Png:
                return true;
            case EncodedImageFormat.Wbmp:
                return false;
            case EncodedImageFormat.Webp:
                return true;
            case EncodedImageFormat.Pkm:
                return false;
            case EncodedImageFormat.Ktx:
                return false;
            case EncodedImageFormat.Astc:
                return false; // not sure if astc is supported at all
            case EncodedImageFormat.Dng:
                return false; // not sure
            case EncodedImageFormat.Heif:
                return true;
            case EncodedImageFormat.Unknown:
                return false;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }
}
