using Avalonia.Media;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Models.Files;

internal class PngFileType : ImageFileType
{
    public static PngFileType PngFile { get; } = new PngFileType();

    public override string DisplayName => new LocalizedString("PNG_FILE");
    public override EncodedImageFormat EncodedImageFormat { get; } = EncodedImageFormat.Png;
    public override string[] Extensions => new[] { ".png" };

    public override SolidColorBrush EditorColor { get; } = new SolidColorBrush(new Color(255, 56, 108, 254));
}
