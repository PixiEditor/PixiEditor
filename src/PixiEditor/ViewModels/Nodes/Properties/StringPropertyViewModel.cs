using System.ComponentModel;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class StringPropertyViewModel : NodePropertyViewModel<string>
{
    public string StringValue
    {
        get => Value;
        set => Value = value;
    }
    
    public StringPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
        PropertyChanged += StringPropertyViewModel_PropertyChanged;
    }
    
    private void StringPropertyViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Value))
        {
            OnPropertyChanged(nameof(StringValue));
        }
    }
}
