using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditorPrototype.Models;
internal static class BlendModeEx
{
    public static string EnglishName(this BlendMode mode)
    {
        return mode switch
        {
            BlendMode.Normal => "Normal",
            BlendMode.Darken => "Darken",
            BlendMode.Multiply => "Multiply",
            BlendMode.ColorBurn => "Color Burn",
            BlendMode.Lighten => "Lighten",
            BlendMode.Screen => "Screen",
            BlendMode.ColorDodge => "Color Dodge",
            BlendMode.LinearDodge => "Linear Dodge (Add)",
            BlendMode.Overlay => "Overlay",
            BlendMode.SoftLight => "Soft Light",
            BlendMode.HardLight => "Hard Light",
            BlendMode.Difference => "Difference",
            BlendMode.Exclusion => "Exclusion",
            BlendMode.Hue => "Hue",
            BlendMode.Saturation => "Saturation",
            BlendMode.Luminosity => "Luminosity",
            BlendMode.Color => "Color",
            _ => "<no name>",
        };
    }
}
