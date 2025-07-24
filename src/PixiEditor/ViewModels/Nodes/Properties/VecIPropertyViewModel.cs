using System.ComponentModel;
using Drawie.Numerics;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class VecIPropertyViewModel : NodePropertyViewModel<VecI>
{
    private bool updateBlocker = false;

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

        updateBlocker = true;

        OnPropertyChanged(nameof(XValue));
        OnPropertyChanged(nameof(YValue));

        updateBlocker = false;
    }

    public int XValue
    {
        get => Value.X;
        set
        {
            if (updateBlocker)
                return;

            Value = new VecI(value, Value.Y);
        }
    }

    public int YValue
    {
        get => Value.Y;
        set
        {
            if (updateBlocker)
                return;

            Value = new VecI(Value.X, value);
        }
    }
}
