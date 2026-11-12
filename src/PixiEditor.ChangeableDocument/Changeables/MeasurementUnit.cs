namespace PixiEditor.ChangeableDocument.Changeables;

public abstract class MeasurementUnit
{
    public static MeasurementUnit Pixels { get; } = new PixelUnit();
    public static MeasurementUnit Millimeter { get; } = new MillimeterUnit();
    public static MeasurementUnit Points { get; } = new PointsUnit();
    public static MeasurementUnit Inches { get; } = new InchesUnit();

    public static IReadOnlyList<MeasurementUnit> AllKnown { get; } =
    [
        Pixels, Millimeter, Points, Inches
    ];

    /// <summary>
    ///     A name used for identification, not display.
    /// </summary>
    public abstract string UnitName { get; }
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

public sealed class PixelUnit : MeasurementUnit
{
    public override string UnitName { get; } = "Pixel";
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

public sealed class MillimeterUnit : MeasurementUnit
{
    public const float MmInInch = 25.4f;
    public override string UnitName { get; } = "Millimeter";
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

public sealed class PointsUnit : MeasurementUnit
{
    public override string UnitName { get; } = "Points";
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

public sealed class InchesUnit : MeasurementUnit
{
    public override string UnitName { get; } = "Inches";
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
