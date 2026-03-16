using Avalonia;
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

    public static bool IsIcon(string unicode)
    {
        if (string.IsNullOrEmpty(unicode)) return false;

        // Check if the unicode is in the Private Use Area (PUA) of Unicode, which is where custom icons are usually placed.
        // The PUA ranges from U+E000 to U+F8FF.
        int codePoint = char.ConvertToUtf32(unicode, 0);
        return codePoint is >= 0xE000 and <= 0xF8FF;
    }

    public static IImage ToIcon(string unicode, double size, double rotation)
    {
        if (string.IsNullOrEmpty(unicode)) return null;

        return new IconImage(unicode, pixiPerfectFontFamily, size, Colors.White, rotation);
    }

    public static string? TryGetByName(string? icon)
    {
        if (string.IsNullOrEmpty(icon))
        {
            return null;
        }

        if (Application.Current.Styles.TryGetResource(icon, null, out object resource))
        {
            return resource as string;
        }

        return icon;
    }

}
