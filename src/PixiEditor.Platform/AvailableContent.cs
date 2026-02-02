namespace PixiEditor.Platform;

public class AvailableContent
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Image { get; set; } = string.Empty;
    
    public List<string> Tags { get; set; } = new List<string>();
}
