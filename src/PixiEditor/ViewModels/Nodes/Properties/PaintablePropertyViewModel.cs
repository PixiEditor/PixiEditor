using Avalonia.Media;
using PixiEditor.Helpers.Extensions;
using Drawie.Backend.Core.ColorsImpl.Paintables;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class PaintablePropertyViewModel : NodePropertyViewModel<Paintable>
{
    private bool enableGradients = true;

    public bool EnableGradients
    {
        get => enableGradients;
        set => SetProperty(ref enableGradients, value);
    }

    public new IBrush Value
    {
        get => base.Value.ToBrush();
        set => base.Value = value.ToPaintable();
    }

    public PaintablePropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }
}
