using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.ViewModels.Nodes;

public class NodeTypeInfo
{
    public string DisplayName { get; }
    
    public string? PickerName { get; }

    public string Category { get; }

    public LocalizedString FinalPickerName { get; }

    public bool Hidden => PickerName is { Length: 0 };
    
    public Type NodeViewModelType { get; }
    
    public Type NodeType { get; }
    
    public string Icon { get; }

    public NodeTypeInfo(Type viewModelType)
    {
        NodeViewModelType = viewModelType;
        NodeType = GetNodeType(NodeViewModelType.BaseType);
        
        var attribute = viewModelType.GetCustomAttribute<NodeViewModelAttribute>();

        DisplayName = attribute.DisplayName;
        PickerName = attribute.PickerName;
        Category = attribute.Category ?? "";
        Icon = attribute.Icon;

        FinalPickerName = (PickerName ?? DisplayName);
    }

    private Type GetNodeType(Type? baseType)
    {
        while (baseType != null)
        {
            if (baseType.IsGenericType)
            {
                var genericArgument = baseType.GetGenericArguments()[0];

                if (genericArgument.IsAssignableTo(typeof(Node)))
                {
                    return genericArgument;
                }
            }

            baseType = baseType.BaseType;
        }

        throw new NullReferenceException($"Could not find node type of '{baseType}' in base classes");
    }
}
