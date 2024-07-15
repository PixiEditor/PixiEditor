using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties;

internal class ColorPropertyViewModel : NodePropertyViewModel<Color>
{
    public ColorPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }
}
