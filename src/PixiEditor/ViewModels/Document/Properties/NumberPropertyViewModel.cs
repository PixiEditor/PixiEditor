using CommunityToolkit.Mvvm.ComponentModel;

namespace PixiEditor.ViewModels.Document.Properties;

internal class NumberPropertyViewModel : PropertyViewModel<double>
{
    public NumberPropertyViewModel(string name, string label,  Action<double, UpdateState> syncBack, double defaultValue = 0) : base(name, syncBack, defaultValue)
    {
        Label = label;
    }
}
