using CommunityToolkit.Mvvm.ComponentModel;
using Drawie.Numerics;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

public partial class RulerOverlayViewModel : ViewModelBase, IRulerOverlayHandler
{
    [ObservableProperty] public partial float Dpi { get; set; }

    [ObservableProperty] public partial bool IsEnabled { get; set; }

    [ObservableProperty] public partial RulerUnit Unit { get; set; } = RulerUnit.Pixels;

    public RulerOverlayViewModel(float dpi)
    {
        Dpi = dpi;
    }
}

public abstract class RulerUnit
{
    public static RulerUnit Pixels { get; } = new PixelUnit();
    public static RulerUnit Millimeter { get; } = new MillimeterUnit();
    public static RulerUnit Points { get; } = new PointsUnit();
    public static RulerUnit Inches { get; } = new InchesUnit();

    public abstract string ShortNameLocalizationKey { get; }
    public abstract bool AllowFractionalValues { get; }

    public abstract double ToPixels(double value, float dpi);
    public abstract double FromPixels(double pixels, float dpi);

    public virtual double GetSpacing(double minSpacing)
    {
        double[] preferredSteps = { 1, 2, 5 };

        double exponent = Math.Floor(Math.Log10(minSpacing));
        double magnitude = Math.Pow(10, exponent);
        double normalized = minSpacing / magnitude;

        foreach (double step in preferredSteps)
        {
            if (normalized <= step)
                return step * magnitude;
        }

        return preferredSteps[0] * magnitude * 10;
    }
}

public class PixelUnit : RulerUnit
{
    public override string ShortNameLocalizationKey { get; } = "PX_UNIT_SHORT_NAME";
    public override bool AllowFractionalValues { get; } = false;

    public override double ToPixels(double value, float dpi)
    {
        return value;
    }

    public override double FromPixels(double pixels, float dpi)
    {
        return pixels;
    }
}

public class MillimeterUnit : RulerUnit
{
    public const float MmInInch = 25.4f;
    public override string ShortNameLocalizationKey { get; } = "MILLIMETER_UNIT_SHORT_NAME";
    public override bool AllowFractionalValues { get; } = true;

    public override double ToPixels(double value, float dpi)
    {
        return (value / dpi) * MmInInch;
    }

    public override double FromPixels(double pixels, float dpi)
    {
        return pixels * (dpi / MmInInch);
    }
}

public class PointsUnit : RulerUnit
{
    public override string ShortNameLocalizationKey { get; } = "POINTS_UNIT_SHORT_NAME";
    public override bool AllowFractionalValues { get; } = true;

    public override double ToPixels(double value, float dpi)
    {
        return (value / dpi) * 72;
    }

    public override double FromPixels(double pixels, float dpi)
    {
        return pixels * (dpi / 72);
    }
}

public class InchesUnit : RulerUnit
{
    public override string ShortNameLocalizationKey { get; } = "INCHES_UNIT_SHORT_NAME";
    public override bool AllowFractionalValues { get; } = true;

    public override double ToPixels(double value, float dpi)
    {
        return value * dpi;
    }

    public override double FromPixels(double pixels, float dpi)
    {
        return pixels / dpi;
    }

    public override double GetSpacing(double minSpacing)
    {
        return Math.Pow(2, Math.Ceiling(Math.Log2(minSpacing * 128))) / 128;
    }
}
