namespace PixiEditor.Platform;

public class AvailableContent
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Author { get; set; } = string.Empty;
    
    public string ImageUrl { get; set; } = string.Empty;
    
    public int Price { get; set; }
    
    public string Currency { get; set; } = string.Empty;

    public List<string> IncludedExtensions = new List<string>();
}
