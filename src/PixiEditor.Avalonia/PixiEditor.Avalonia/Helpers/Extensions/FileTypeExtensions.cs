using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Models.Files;

namespace PixiEditor.Avalonia.Helpers.Extensions;

public static class FileTypeExtensions
{
    public static EncodedImageFormat ToEncodedImageFormat(this FileType fileType)
    {
        return fileType switch
        {
            FileType.Png => EncodedImageFormat.Png,
            FileType.Jpeg => EncodedImageFormat.Jpeg,
            FileType.Bmp => EncodedImageFormat.Bmp,
            FileType.Gif => EncodedImageFormat.Gif,
            _ => EncodedImageFormat.Unknown
        };
    }
}
