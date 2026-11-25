using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.ViewModels.Document.Properties;

internal class CustomSizePropertyViewModel : PropertyViewModel<double>
{
    public MeasurementUnit MeasurementUnit
    {
        get;
        set => SetProperty(ref field, value);
    }

    public MeasurementConverterData ConverterData
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double Min
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Unit
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int Decimals
    {
        get;
        set => SetProperty(ref field, value);
    }

    public CustomSizePropertyViewModel(string name, string label, Action<double, UpdateState> syncBack, double defaultValue = 0) : base(name, syncBack, defaultValue)
    {
        Label = label;
    }
}
