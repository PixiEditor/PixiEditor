using System.ComponentModel;
using Drawie.Numerics;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class Vec3DPropertyViewModel : NodePropertyViewModel<Vec3D>
{
    private bool updateBlocker = false;
    public Vec3DPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
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
        OnPropertyChanged(nameof(ZValue));
        updateBlocker = false;
    }

    public double XValue
    {
        get => Value.X;
        set
        {
            if (updateBlocker)
                return;
            Value = new Vec3D(value, Value.Y, Value.Z);
        }
    }

    public double YValue
    {
        get => Value.Y;
        set
        {
            if (updateBlocker)
                return;
            Value = new Vec3D(Value.X, value, Value.Z);
        }
    }

    public double ZValue
    {
        get => Value.Z;
        set
        {
            if (updateBlocker)
                return;
            Value = new Vec3D(Value.X, Value.Y, value);
        }
    }
}
