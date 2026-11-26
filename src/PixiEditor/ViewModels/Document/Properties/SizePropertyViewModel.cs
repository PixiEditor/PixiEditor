using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.ViewModels.Document.Properties;

internal class SizePropertyViewModel : PropertyViewModel<double>
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
    } = 0.00001;

    public SizePropertyViewModel(string name, string label, Action<double, UpdateState> syncBack, double defaultValue = 0) : base(name, syncBack, defaultValue)
    {
        Label = label;
    }
}
