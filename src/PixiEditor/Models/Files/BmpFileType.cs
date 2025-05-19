using Avalonia.Media;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Files;

internal class BmpFileType : ImageFileType
{
    public static BmpFileType BmpFile { get; } = new BmpFileType();
    public override string[] Extensions { get; } = new[] { ".bmp" };
    public override string DisplayName => new LocalizedString("BMP_FILE");
    public override EncodedImageFormat EncodedImageFormat { get; } = EncodedImageFormat.Bmp;
    public override SolidColorBrush EditorColor { get; } = new SolidColorBrush(new Color(255, 255, 140, 0));
}
