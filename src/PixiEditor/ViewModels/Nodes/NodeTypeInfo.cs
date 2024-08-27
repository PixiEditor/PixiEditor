using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Fonts;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.ViewModels.Nodes;

public class NodeTypeInfo
{
    public string UniqueName { get; }
    
    public string DisplayName { get; }
    
    public string? PickerName { get; }

    public string Category { get; }

    public LocalizedString FinalPickerName { get; }

    public bool Hidden => PickerName is { Length: 0 };
    
    public Type NodeType { get; }
    
    public string Icon { get; }

    public NodeTypeInfo(Type type)
    {
        NodeType = type;

        var attribute = type.GetCustomAttribute<NodeInfoAttribute>();

        UniqueName = attribute.UniqueName;
        DisplayName = attribute.DisplayName;
        PickerName = attribute.PickerName;
        Category = attribute.Category ?? "";

        if (NodeIcons.IconMap.TryGetValue(type, out var icon))
        {
            Icon = icon;
        }

        FinalPickerName = (PickerName ?? DisplayName);
    }
}
