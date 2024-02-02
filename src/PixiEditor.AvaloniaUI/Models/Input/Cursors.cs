using Avalonia;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Helpers.Converters;

namespace PixiEditor.AvaloniaUI.Models.Input;

public static class Cursors
{
    public static Cursor PreciseCursor { get; } = new Cursor(
        ImagePathToBitmapConverter.LoadBitmapFromRelativePath("/Images/Tools/PreciseCursor.png"),
        new PixelPoint(16, 16));
}
