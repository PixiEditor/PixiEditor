using Avalonia.Media;
using PixiEditor.DrawingApi.Core.Surfaces.Surface;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Models.Files;

internal class JpegFileType : ImageFileType
{
    public static JpegFileType JpegFile { get; } = new JpegFileType();

    public override string[] Extensions => new[] { ".jpeg", ".jpg" };
    public override string DisplayName => new LocalizedString("JPEG_FILE");
    public override EncodedImageFormat EncodedImageFormat { get; } = EncodedImageFormat.Jpeg;

    public override SolidColorBrush EditorColor { get; } = new SolidColorBrush(new Color(255, 36, 179, 66));
}
