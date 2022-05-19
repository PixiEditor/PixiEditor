using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Rendering;
internal class RenderingContext : IDisposable
{
    public SKPaint BlendModePaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
    public SKPaint BlendModeOpacityPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
    public SKPaint ReplacingPaintWithOpacity = new SKPaint() { BlendMode = SKBlendMode.Src };

    public void UpdateFromMember(IReadOnlyStructureMember member)
    {
        SKColor opacityColor = new(255, 255, 255, (byte)Math.Round(member.Opacity * 255));
        SKBlendMode blendMode = GetSKBlendMode(member.BlendMode);

        BlendModeOpacityPaint.Color = opacityColor;
        BlendModeOpacityPaint.BlendMode = blendMode;
        BlendModePaint.BlendMode = blendMode;
        ReplacingPaintWithOpacity.Color = opacityColor;
    }

    private static SKBlendMode GetSKBlendMode(BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.Normal => SKBlendMode.SrcOver,
            BlendMode.Darken => SKBlendMode.Darken,
            BlendMode.Multiply => SKBlendMode.Multiply,
            BlendMode.ColorBurn => SKBlendMode.ColorBurn,
            BlendMode.Lighten => SKBlendMode.Lighten,
            BlendMode.Screen => SKBlendMode.Screen,
            BlendMode.ColorDodge => SKBlendMode.ColorDodge,
            BlendMode.LinearDodge => SKBlendMode.Plus,
            BlendMode.Overlay => SKBlendMode.Overlay,
            BlendMode.SoftLight => SKBlendMode.SoftLight,
            BlendMode.HardLight => SKBlendMode.HardLight,
            BlendMode.Difference => SKBlendMode.Difference,
            BlendMode.Exclusion => SKBlendMode.Exclusion,
            BlendMode.Hue => SKBlendMode.Hue,
            BlendMode.Saturation => SKBlendMode.Saturation,
            BlendMode.Luminosity => SKBlendMode.Luminosity,
            BlendMode.Color => SKBlendMode.Color,
            _ => SKBlendMode.SrcOver,
        };
    }

    public void Dispose()
    {
        BlendModePaint.Dispose();
        BlendModeOpacityPaint.Dispose();
        ReplacingPaintWithOpacity.Dispose();
    }
}
