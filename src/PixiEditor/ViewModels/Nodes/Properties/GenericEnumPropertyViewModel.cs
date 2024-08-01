using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class GenericEnumPropertyViewModel : NodePropertyViewModel
{
    public GenericEnumPropertyViewModel(INodeHandler node, Type propertyType, Type enumType) : base(node, propertyType)
    {
        Values = Enum.GetValues(enumType);
    }

    public Array Values { get; }
}
