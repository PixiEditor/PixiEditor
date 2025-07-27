using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace PixiEditor.Helpers.Converters;

public class TwoWayFrameToTimeConverter : AvaloniaObject, IValueConverter
{
    public static readonly StyledProperty<int> FpsProperty = AvaloniaProperty.Register<TwoWayFrameToTimeConverter, int>(
        nameof(Fps));

    public int Fps
    {
        get => GetValue(FpsProperty);
        set => SetValue(FpsProperty, value);
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int frame)
        {
            var span = TimeSpan.FromSeconds(frame / (double)Fps).ToString("mm\\:ss\\.ff");
            return span;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string timeString)
        {
            if (TimeSpan.TryParse(timeString, CultureInfo.InvariantCulture, out TimeSpan time))
            {
                return (int)(time.TotalSeconds * Fps);
            }

            if(TimeSpan.TryParseExact(timeString, "mm\\:ss\\.ff", CultureInfo.InvariantCulture, out time))
            {
                return (int)(time.TotalSeconds * Fps);
            }

            if (timeString.EndsWith("s"))
            {
                timeString = timeString.TrimEnd('s').Trim();
                if (double.TryParse(timeString, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
                {
                    return (int)(seconds * Fps);
                }
            }
        }

        return null;
    }
}
