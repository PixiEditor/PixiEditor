using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class GenericPropertyViewModel : NodePropertyViewModel
{
    public GenericPropertyViewModel(INodeHandler node, Type valueType) : base(node, valueType)
    {
    }
}
