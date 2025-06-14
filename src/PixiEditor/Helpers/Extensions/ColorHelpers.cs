using Avalonia;
using Avalonia.Media;
using Avalonia.Skia;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using Drawie.Skia;
using PixiEditor.Extensions.CommonApi.Palettes;
using BackendColor = Drawie.Backend.Core.ColorsImpl.Color;
using GradientStop = Drawie.Backend.Core.ColorsImpl.Paintables.GradientStop;

namespace PixiEditor.Helpers.Extensions;

internal static class ColorHelpers
{
    public static BackendColor ToOpaqueColor(this Color color) => new(color.R, color.G, color.B);
    public static BackendColor ToColor(this Color color) => new(color.R, color.G, color.B, color.A);
    public static BackendColor ToColor(this PaletteColor color) => new(color.R, color.G, color.B, 255);

    public static PaletteColor ToPaletteColor(this Color color) => new(color.R, color.G, color.B);
    public static PaletteColor ToPaletteColor(this BackendColor color) => new(color.R, color.G, color.B);

    public static Color ToOpaqueMediaColor(this BackendColor color) => Color.FromRgb(color.R, color.G, color.B);
    public static Color ToColor(this BackendColor color) => Color.FromArgb(color.A, color.R, color.G, color.B);
    public static Color ToMediaColor(this PaletteColor color) => Color.FromRgb(color.R, color.G, color.B);

    public static BackendColor BlendColors(BackendColor bottomColor, BackendColor topColor)
    {
        if (topColor.A is < 255 and > 0)
        {
            byte r = (byte)((topColor.R * topColor.A / 255) +
                            (bottomColor.R * bottomColor.A * (255 - topColor.A) / (255 * 255)));
            byte g = (byte)((topColor.G * topColor.A / 255) +
                            (bottomColor.G * bottomColor.A * (255 - topColor.A) / (255 * 255)));
            byte b = (byte)((topColor.B * topColor.A / 255) +
                            (bottomColor.B * bottomColor.A * (255 - topColor.A) / (255 * 255)));
            byte a = (byte)(topColor.A + (bottomColor.A * (255 - topColor.A) / 255));
            return new BackendColor(r, g, b, a);
        }

        return topColor.A == 255 ? topColor : bottomColor;
    }

    public static Paintable ToPaintable(this IBrush avaloniaBrush) => avaloniaBrush switch
    {
        ISolidColorBrush solidColorBrush => new BackendColor(solidColorBrush.Color.R, solidColorBrush.Color.G,
            solidColorBrush.Color.B, solidColorBrush.Color.A),
        ILinearGradientBrush linearGradientBrush =>
            new LinearGradientPaintable(
                new VecD(linearGradientBrush.StartPoint.Point.X, linearGradientBrush.StartPoint.Point.Y),
                new VecD(linearGradientBrush.EndPoint.Point.X, linearGradientBrush.EndPoint.Point.Y),
                linearGradientBrush.GradientStops.Select(stop =>
                    new GradientStop(new BackendColor(stop.Color.R, stop.Color.G, stop.Color.B, stop.Color.A), stop.Offset)))
            {
                AbsoluteValues = linearGradientBrush.StartPoint.Unit == RelativeUnit.Absolute ||
                                 linearGradientBrush.EndPoint.Unit == RelativeUnit.Absolute,
                Transform = linearGradientBrush.Transform != null ? ToDrawieMatrix(linearGradientBrush.Transform.Value) : null
            },
        IRadialGradientBrush radialGradientBrush => new RadialGradientPaintable(
            new VecD(radialGradientBrush.Center.Point.X, radialGradientBrush.Center.Point.Y),
            radialGradientBrush.RadiusX.Scalar,
            radialGradientBrush.GradientStops.Select(stop =>
                new GradientStop(new BackendColor(stop.Color.R, stop.Color.G, stop.Color.B, stop.Color.A), stop.Offset)))
        {
            AbsoluteValues = radialGradientBrush.Center.Unit == RelativeUnit.Absolute ||
                             radialGradientBrush.RadiusX.Unit == RelativeUnit.Absolute,
            Transform = radialGradientBrush.Transform != null ? ToDrawieMatrix(radialGradientBrush.Transform.Value) : null
        },
        IConicGradientBrush conicGradientBrush => new SweepGradientPaintable(
            new VecD(conicGradientBrush.Center.Point.X, conicGradientBrush.Center.Point.Y),
            conicGradientBrush.Angle,
            conicGradientBrush.GradientStops.Select(stop =>
                new GradientStop(new BackendColor(stop.Color.R, stop.Color.G, stop.Color.B, stop.Color.A), stop.Offset)))
        {
            AbsoluteValues = conicGradientBrush.Center.Unit == RelativeUnit.Absolute,
            Transform = conicGradientBrush.Transform != null ? ToDrawieMatrix(conicGradientBrush.Transform.Value) : null
        },
        null => null,

    };

    public static IBrush ToBrush(this Paintable paintable) => paintable switch
    {
        ColorPaintable color => new SolidColorBrush(color.Color.ToColor()),
        LinearGradientPaintable linearGradientPaintable => new LinearGradientBrush
        {
            StartPoint = new RelativePoint(linearGradientPaintable.Start.X, linearGradientPaintable.Start.Y, paintable.AbsoluteValues ? RelativeUnit.Absolute : RelativeUnit.Relative),
            EndPoint = new RelativePoint(linearGradientPaintable.End.X, linearGradientPaintable.End.Y, paintable.AbsoluteValues ? RelativeUnit.Absolute : RelativeUnit.Relative),
            GradientStops = ToAvaloniaGradientStops(linearGradientPaintable.GradientStops),
            Transform = linearGradientPaintable.Transform.HasValue ? new MatrixTransform(ToAvaloniaMatrix(linearGradientPaintable.Transform.Value)) : null
        },
        RadialGradientPaintable radialGradientPaintable => new RadialGradientBrush
        {
            Center = new RelativePoint(radialGradientPaintable.Center.X, radialGradientPaintable.Center.Y, paintable.AbsoluteValues ? RelativeUnit.Absolute : RelativeUnit.Relative),
            RadiusX = new RelativeScalar(radialGradientPaintable.Radius, RelativeUnit.Absolute),
            RadiusY = new RelativeScalar(radialGradientPaintable.Radius, RelativeUnit.Absolute),
            GradientStops = ToAvaloniaGradientStops(radialGradientPaintable.GradientStops),
            Transform = radialGradientPaintable.Transform.HasValue ? new MatrixTransform(ToAvaloniaMatrix(radialGradientPaintable.Transform.Value)) : null
        },
        SweepGradientPaintable conicGradientPaintable => new ConicGradientBrush
        {
            Angle = conicGradientPaintable.Angle,
            Center = new RelativePoint(conicGradientPaintable.Center.X, conicGradientPaintable.Center.Y, paintable.AbsoluteValues ? RelativeUnit.Absolute : RelativeUnit.Relative),
            GradientStops = ToAvaloniaGradientStops(conicGradientPaintable.GradientStops),
            Transform = conicGradientPaintable.Transform.HasValue ? new MatrixTransform(ToAvaloniaMatrix(conicGradientPaintable.Transform.Value)) : null
        },
        null => null,
        _ => throw new NotImplementedException()
    };

    private static GradientStops ToAvaloniaGradientStops(IEnumerable<GradientStop> gradientStops)
    {
        GradientStops stops = new GradientStops();
        foreach (var stop in gradientStops)
        {
            stops.Add(new Avalonia.Media.GradientStop(stop.Color.ToColor(), stop.Offset));
        }

        return stops;
    }

    private static Matrix ToAvaloniaMatrix(Matrix3X3 matrix)
    {
        return new Matrix(matrix.ScaleX, matrix.SkewY, matrix.SkewX, matrix.ScaleY, matrix.TransX, matrix.TransY);
    }

    private static Matrix3X3 ToDrawieMatrix(Matrix matrix)
    {
        return matrix.ToSKMatrix().ToMatrix3X3();
    }
}
