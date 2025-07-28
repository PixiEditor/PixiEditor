using Avalonia.Media;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Files;

internal class GifFileType : VideoFileType
{
    public static GifFileType GifFile { get; } = new GifFileType();
    public override string[] Extensions { get; } = new[] { ".gif" };
    public override string DisplayName => new LocalizedString("GIF_FILE");
    public override SolidColorBrush EditorColor { get; } = new SolidColorBrush(new Color(255, 180, 0, 255));
}
