namespace PixiEditor.ChangeableDocument.Changeables.Graph;

[AttributeUsage(AttributeTargets.Class)]
public class NodeInfoAttribute : Attribute
{
    public string UniqueName { get; }
    
    public string DisplayName { get; }
    
    public string? PickerName { get; set; }

    public NodeInfoAttribute(string uniqueName, string displayName)
    {
        if (!uniqueName.StartsWith("PixiEditor"))
        {
            uniqueName = $"PixiEditor.{uniqueName}";
        }
        
        UniqueName = uniqueName;
        DisplayName = displayName;
    }
}
