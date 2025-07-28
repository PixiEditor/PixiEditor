using System.ComponentModel;
using Drawie.Numerics;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class VecDPropertyViewModel : NodePropertyViewModel<VecD>
{
    private bool updateBlocker = false;
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

        updateBlocker = true;
        
        OnPropertyChanged(nameof(XValue));
        OnPropertyChanged(nameof(YValue));

        updateBlocker = false;
    }

    public double XValue
    {
        get => Value.X;
        set
        {
            if (updateBlocker)
                return;
            Value = new VecD(value, Value.Y);
        }
    }

    public double YValue
    {
        get => Value.Y;
        set
        {
            if (updateBlocker)
                return;

            Value = new VecD(Value.X, value);
        }
    }
}
