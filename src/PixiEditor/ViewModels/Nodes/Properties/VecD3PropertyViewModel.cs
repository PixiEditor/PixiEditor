using System.ComponentModel;
using PixiEditor.Numerics;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class VecD3PropertyViewModel : NodePropertyViewModel<VecD3>
{
    public VecD3PropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
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
        OnPropertyChanged(nameof(ZValue));
    }

    public double XValue
    {
        get => Value.X;
        set => Value = new VecD3(value, Value.Y, Value.Z);
    }
    
    public double YValue
    {
        get => Value.Y;
        set => Value = new VecD3(Value.X, value, Value.Z);
    }
    
    public double ZValue
    {
        get => Value.Z;
        set => Value = new VecD3(Value.X, Value.Y, value);
    }
}
