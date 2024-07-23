using System.ComponentModel;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties;

internal class VecDPropertyViewModel : NodePropertyViewModel<VecD>
{
    public VecDPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
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

    public double XValue
    {
        get => Value.X;
        set => Value = new VecD(value, Value.Y);
    }
    
    public double YValue
    {
        get => Value.Y;
        set => Value = new VecD(Value.X, value);
    }
}
