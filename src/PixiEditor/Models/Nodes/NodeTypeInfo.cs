using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;

namespace PixiEditor.Models.Nodes;

public class NodeTypeInfo
{
    public string UniqueName { get; }
    
    public string DisplayName { get; }
    
    public string? PickerName { get; }

    public string FinalPickerName => PickerName ?? DisplayName;
    
    public Type NodeType { get; }

    public NodeTypeInfo(Type type)
    {
        NodeType = type;

        var attribute = type.GetCustomAttribute<NodeInfoAttribute>();

        UniqueName = attribute.UniqueName;
        DisplayName = attribute.DisplayName;
        PickerName = attribute.PickerName;
    }
}
