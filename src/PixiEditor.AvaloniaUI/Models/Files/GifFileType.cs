using Avalonia.Media;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Models.Files;

internal class GifFileType : ImageFileType
{
    public static GifFileType GifFile { get; } = new GifFileType();
    public override string[] Extensions { get; } = new[] { ".gif" };
    public override string DisplayName => new LocalizedString("GIF_FILE");
    public override EncodedImageFormat EncodedImageFormat { get; } = EncodedImageFormat.Gif;
    
    public override SolidColorBrush EditorColor { get; } = new SolidColorBrush(new Color(255, 180, 0, 255));
}
