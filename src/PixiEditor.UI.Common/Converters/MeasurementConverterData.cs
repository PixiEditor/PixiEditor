using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixiEditor.UI.Common.Converters;

public class MeasurementConverterData : INotifyPropertyChanged
{
    public Func<float> GetDpi { get; }
    public Func<double, float, double> ToPixels { get; }
    public Func<double, float, double> FromPixels { get; }
    public Func<int> GetDecimals { get; }
    public int Decimals => GetDecimals?.Invoke() ?? 0;

    public MeasurementConverterData(Func<float> getDpi, Func<double, float, double> fromPixels, Func<double, float, double> toPixels, Func<int> getDecimals)
    {
        GetDpi = getDpi;
        ToPixels = toPixels;
        FromPixels = fromPixels;
        GetDecimals = getDecimals;
    }

    public void NotifyUpdated()
    {
        OnPropertyChanged(nameof(GetDpi));
        OnPropertyChanged(nameof(ToPixels));
        OnPropertyChanged(nameof(FromPixels));
        OnPropertyChanged(nameof(GetDecimals));
        OnPropertyChanged(nameof(Decimals));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
