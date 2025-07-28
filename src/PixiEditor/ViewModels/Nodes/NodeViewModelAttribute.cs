namespace PixiEditor.ViewModels.Nodes;

public class NodeViewModelAttribute(string displayName, string? category, string? icon) : Attribute
{
    public string DisplayName { get; } = displayName;

    public string? Category { get; } = category;

    public string? Icon { get; } = icon;

    public string? PickerName { get; set; }
}
