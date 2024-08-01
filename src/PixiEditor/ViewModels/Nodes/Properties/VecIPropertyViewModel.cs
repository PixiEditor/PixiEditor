using System.ComponentModel;
using PixiEditor.Numerics;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class VecIPropertyViewModel : NodePropertyViewModel<VecI>
{
    public VecIPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Value))
        {
            return;
        }
        
        OnPropertyChanged(nameof(XValue));
        OnPropertyChanged(nameof(YValue));
    }

    public int XValue
    {
        get => Value.X;
        set => Value = new VecI(value, Value.Y);
    }
    
    public int YValue
    {
        get => Value.Y;
        set => Value = new VecI(Value.X, value);
    }
}
