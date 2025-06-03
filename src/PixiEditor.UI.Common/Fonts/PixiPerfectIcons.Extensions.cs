using Avalonia.Media;
using Avalonia.Platform;
using PixiEditor.UI.Common.Rendering;

namespace PixiEditor.UI.Common.Fonts;

public static class PixiPerfectIconExtensions
{
    private static readonly FontFamily pixiPerfectFontFamily =
        new("avares://PixiEditor.UI.Common/Fonts/PixiPerfect.ttf#pixiperfect");


    public static Stream GetFontStream()
    {
        return AssetLoader.Open(new Uri("avares://PixiEditor.UI.Common/Fonts/PixiPerfect.ttf"));
    }

    public static IImage ToIcon(string unicode, double size = 18)
    {
        if (string.IsNullOrEmpty(unicode)) return null;

        return new IconImage(unicode, pixiPerfectFontFamily, size, Colors.White);
    }

    public static IImage ToIcon(string unicode, double size, double rotation)
    {
        if (string.IsNullOrEmpty(unicode)) return null;

        return new IconImage(unicode, pixiPerfectFontFamily, size, Colors.White, rotation);
    }
}
