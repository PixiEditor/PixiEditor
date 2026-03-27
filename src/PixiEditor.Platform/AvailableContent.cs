namespace PixiEditor.Platform;

public class AvailableContent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Body { get; set; }
    public string Author { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string[] VideoUrls { get; set; }
    public int Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool HideAddToLibrary { get; set; }
    public bool IsBundle { get; set; }
    public List<string> IncludedExtensions { get; set; } = new List<string>();
}
