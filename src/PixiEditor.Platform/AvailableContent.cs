namespace PixiEditor.Platform;

public class AvailableContent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Body { get; set; }
    public string Author { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string[] ShowcaseUrls { get; set; }
    public int Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int PercentageDiscount { get; set; }
    public bool HideAddToLibrary { get; set; }
    public bool IsBundle { get; set; }
    public List<string> IncludedExtensions { get; set; } = new List<string>();
    public DateTime ReleaseDate { get; set; }
    public List<ExtensionVersion> Versions { get; set; } = new List<ExtensionVersion>();
}

public class ExtensionVersion
{
    public string Version { get; set; } = string.Empty;
    public int PixiEditorApiVersion { get; set; }
}
