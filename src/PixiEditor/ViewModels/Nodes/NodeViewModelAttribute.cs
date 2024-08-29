namespace PixiEditor.ViewModels.Nodes;

public class NodeViewModelAttribute(string displayName, string? category) : Attribute
{
    public string DisplayName { get; } = displayName;

    public string? Category { get; } = category;
    
    public string? PickerName { get; set; }
}
