using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class ColorPropertyViewModel : NodePropertyViewModel<Color>
{
    private bool enableGradients = false;

    public bool EnableGradients
    {
        get => enableGradients;
        set => SetProperty(ref enableGradients, value);
    }

    public new Avalonia.Media.Color Value
    {
        get => base.Value.ToColor();
        set => base.Value = value.ToColor();
    }

    public ColorPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }
}
