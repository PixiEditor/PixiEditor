using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Localization;

namespace PixiEditor.Helpers.Extensions;
internal static class BlendModeEx
{
    public static string LocalizedName(this BlendMode mode)
    {
        return mode switch
        {
            BlendMode.Normal => new LocalizedString("NORMAL_BLEND_MODE"),
            BlendMode.Darken => new LocalizedString("DARKEN_BLEND_MODE"),
            BlendMode.Multiply => new LocalizedString("MULTIPLY_BLEND_MODE"),
            BlendMode.ColorBurn => new LocalizedString("COLOR_BURN_BLEND_MODE"),
            BlendMode.Lighten => new LocalizedString("LIGHTEN_BLEND_MODE"),
            BlendMode.Screen => new LocalizedString("SCREEN_BLEND_MODE"),
            BlendMode.ColorDodge => new LocalizedString("COLOR_DODGE_BLEND_MODE"),
            BlendMode.LinearDodge => new LocalizedString("LINEAR_DODGE_BLEND_MODE"),
            BlendMode.Overlay => new LocalizedString("OVERLAY_BLEND_MODE"),
            BlendMode.SoftLight => new LocalizedString("SOFT_LIGHT_BLEND_MODE"),
            BlendMode.HardLight => new LocalizedString("HARD_LIGHT_BLEND_MODE"),
            BlendMode.Difference => new LocalizedString("DIFFERENCE_BLEND_MODE"),
            BlendMode.Exclusion => new LocalizedString("EXCLUSION_BLEND_MODE"),
            BlendMode.Hue => new LocalizedString("HUE_BLEND_MODE"),
            BlendMode.Saturation => new LocalizedString("SATURATION_BLEND_MODE"),
            BlendMode.Luminosity => new LocalizedString("LUMINOSITY_BLEND_MODE"),
            BlendMode.Color => new LocalizedString("COLOR_BLEND_MODE"),
            _ => "NOT_SUPPORTED_BLEND_MODE"
        };
    }
}
