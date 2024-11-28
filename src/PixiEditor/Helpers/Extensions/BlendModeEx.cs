using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.Helpers.Extensions;
internal static class BlendModeEx
{
    public static string LocalizedKeys(this BlendMode mode)
    {
        return mode switch
        {
            BlendMode.Normal => "NORMAL_BLEND_MODE",
            BlendMode.Erase => "ERASE_BLEND_MODE",
            BlendMode.Darken => "DARKEN_BLEND_MODE",
            BlendMode.Multiply => "MULTIPLY_BLEND_MODE",
            BlendMode.ColorBurn => "COLOR_BURN_BLEND_MODE",
            BlendMode.Lighten => "LIGHTEN_BLEND_MODE",
            BlendMode.Screen => "SCREEN_BLEND_MODE",
            BlendMode.ColorDodge => "COLOR_DODGE_BLEND_MODE",
            BlendMode.LinearDodge => "LINEAR_DODGE_BLEND_MODE",
            BlendMode.Overlay => "OVERLAY_BLEND_MODE",
            BlendMode.SoftLight => "SOFT_LIGHT_BLEND_MODE",
            BlendMode.HardLight => "HARD_LIGHT_BLEND_MODE",
            BlendMode.Difference => "DIFFERENCE_BLEND_MODE",
            BlendMode.Exclusion => "EXCLUSION_BLEND_MODE",
            BlendMode.Hue => "HUE_BLEND_MODE",
            BlendMode.Saturation => "SATURATION_BLEND_MODE",
            BlendMode.Luminosity => "LUMINOSITY_BLEND_MODE",
            BlendMode.Color => "COLOR_BLEND_MODE",
            _ => "NOT_SUPPORTED_BLEND_MODE"
        };
    }
}
