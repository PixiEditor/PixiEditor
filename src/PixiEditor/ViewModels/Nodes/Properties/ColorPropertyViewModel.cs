using PixiEditor.Helpers.Extensions;
using Drawie.Backend.Core.ColorsImpl;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class ColorPropertyViewModel : NodePropertyViewModel<Color>
{
    public ColorPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }

    public new Avalonia.Media.Color Value
    {
        get => base.Value.ToColor();
        set => base.Value = value.ToColor();
    }
}
