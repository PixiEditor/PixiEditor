using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.Models.Nodes;

public class NodeTypeInfo
{
    public string UniqueName { get; }
    
    public string DisplayName { get; }
    
    public string? PickerName { get; }

    public LocalizedString FinalPickerName { get; }

    public bool Hidden => PickerName is { Length: 0 };
    
    public Type NodeType { get; }

    public NodeTypeInfo(Type type)
    {
        NodeType = type;

        var attribute = type.GetCustomAttribute<NodeInfoAttribute>();

        UniqueName = attribute.UniqueName;
        DisplayName = attribute.DisplayName;
        PickerName = attribute.PickerName;

        FinalPickerName = PickerName ?? DisplayName;
    }
}
