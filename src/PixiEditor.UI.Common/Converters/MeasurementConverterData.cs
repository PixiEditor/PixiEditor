namespace PixiEditor.UI.Common.Converters;

public class MeasurementConverterData
{
    public Func<float> GetDpi { get; }
    public Func<double, float, double> ToPixels { get; }
    public Func<double, float, double> FromPixels { get; }

    public MeasurementConverterData(Func<float> getDpi, Func<double, float, double> fromPixels, Func<double, float, double> toPixels)
    {
        GetDpi = getDpi;
        ToPixels = toPixels;
        FromPixels = fromPixels;
    }
}
