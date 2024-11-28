using System.ComponentModel;
using System.Diagnostics.Contracts;
using Drawie.Numerics;

namespace PixiEditor.Models.DocumentModels;

internal record struct ViewportColorChannels
{
    public ColorChannelMode Red { get; }

    public ColorChannelMode Green { get; }

    public ColorChannelMode Blue { get; }

    public ColorChannelMode Alpha { get; }

    public ViewportColorChannels(ColorChannelMode red, ColorChannelMode green, ColorChannelMode blue, ColorChannelMode alpha)
    {
        ReadOnlySpan<ColorChannelMode> modes = [red, green, blue, alpha];
        int solos = 0;

        for (int i = 0; i < modes.Length; i++)
        {
            if (!modes[i].IsSolo)
            {
                continue;
            }

            solos++;

            if (solos > 1)
            {
                throw new ArgumentException("Can't have more than one channel solo");
            }
        }

        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }

    public static ViewportColorChannels Default => new(ColorChannelMode.Default, ColorChannelMode.Default, ColorChannelMode.Default, ColorChannelMode.Default);

    public override string ToString() => $"Red: {Red}; Green: {Green}; Blue: {Blue}; Alpha: {Alpha}";

    public bool IsVisiblyVisible(ColorChannel channel) =>
        GetModeForChannel(channel).IsVisible || GetModeForChannel(channel).IsSolo;

    public bool IsSolo(ColorChannel channel) => GetModeForChannel(channel).IsSolo;

    [Pure]
    public ViewportColorChannels WithModeForChannel(ColorChannel channel, Func<ColorChannelMode, ColorChannelMode> mode, bool otherNonSolo)
    {
        switch (channel)
        {
            case ColorChannel.Red:
                return new ViewportColorChannels(mode(Red), MON(Green), MON(Blue), MON(Alpha));
            case ColorChannel.Green:
                return new ViewportColorChannels(MON(Red), mode(Green), MON(Blue), MON(Alpha));
            case ColorChannel.Blue:
                return new ViewportColorChannels(MON(Red), MON(Green), mode(Blue), MON(Alpha));
            case ColorChannel.Alpha:
                return new ViewportColorChannels(MON(Red), MON(Green), MON(Blue), mode(Alpha));
            case ColorChannel.None:
                throw new InvalidEnumArgumentException(nameof(channel), (int)channel, typeof(ColorChannel));
            default:
                throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
        }

        // Modify Other Node
        ColorChannelMode MON(ColorChannelMode otherMode)
        {
            if (otherNonSolo && otherMode.IsSolo)
            {
                return otherMode.WithSolo(false);
            }

            return otherMode;
        }
    }
    
    public ColorChannelMode GetModeForChannel(ColorChannel channel) => channel switch
    {
        ColorChannel.Red => Red,
        ColorChannel.Green => Green,
        ColorChannel.Blue => Blue,
        ColorChannel.Alpha => Alpha
    };

    public ColorMatrix GetColorMatrix()
    {
        var solo = GetSoloChannel();

        var (otherToRed, redToRed) = GetTarget(Red, solo, ColorChannel.Red);
        var (otherToGreen, greenToGreen) = GetTarget(Green, solo, ColorChannel.Green);
        var (otherToBlue, blueToBlue) = GetTarget(Blue, solo, ColorChannel.Blue);
        
        var opaque = solo is not ColorChannel.None || !Alpha.IsVisible;

        var alphaToOther = Alpha.IsSolo;
        var alphaToAlpha = !alphaToOther && !opaque;

        var o2r = otherToRed ? 1 : 0;
        var r2r = redToRed ? 1 : 0;

        var o2g = otherToGreen ? 1 : 0;
        var g2g = greenToGreen ? 1 : 0;

        var o2b = otherToBlue ? 1 : 0;
        var b2b = blueToBlue ? 1 : 0;

        var a2o = alphaToOther ? 1 : 0;
        var a2a = alphaToAlpha ? 1 : 0;

        var o = opaque ? 1 : 0;

        return new ColorMatrix(
            (r2r, o2g, o2b, a2o, 0),
            (o2r, g2g, o2b, a2o, 0),
            (o2r, o2g, b2b, a2o, 0),
            (0, 0, 0, a2a, o)
        );
    }

    private static (bool otherToRed, bool targetToTarget) GetTarget(ColorChannelMode mode, ColorChannel solo, ColorChannel target)
    {
        var otherToTarget = solo == target;
        var targetToTarget = solo == target || (mode.IsVisible && solo == ColorChannel.None);

        return (otherToTarget, targetToTarget);
    }

    public ColorChannel GetSoloChannel()
    {
        ReadOnlySpan<(ColorChannel channel, ColorChannelMode mode)> modes = [
            (ColorChannel.Red, Red),
            (ColorChannel.Green, Green),
            (ColorChannel.Blue, Blue),
            (ColorChannel.Alpha, Alpha)
        ];
    
        for (int i = 0; i < modes.Length; i++)
        {
            var mode = modes[i];
            if (modes[i].mode.IsSolo)
            {
                return mode.channel;
            }
        }
    
        return ColorChannel.None;
    }
}
