using System.ComponentModel;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class GenericEnumPropertyViewModel : NodePropertyViewModel
{
    public GenericEnumPropertyViewModel(INodeHandler node, Type propertyType, Type enumType) : base(node, propertyType)
    {
        Values = Enum.GetValues(enumType);
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Value))
        {
            OnPropertyChanged(nameof(SelectedIndex));
        }
    }

    public Array Values { get; }
    public int SelectedIndex
    {
        get => Value == null ? -1 : Array.IndexOf(Values, Value);
        set => Value = Values.GetValue(value);
    }
}
