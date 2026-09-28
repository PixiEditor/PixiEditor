namespace PixiEditor.Platform;

public class ExtensionsLayout
{
    public List<HighlightData> HighlightedExtensions { get; set; } = new List<HighlightData>();
    public List<string> FeaturedExtensionIds { get; set; } = new List<string>();
}

public class HighlightData
{
    public string ExtensionId { get; set; } = string.Empty;
    public string HeaderTaglineText  { get; set; } = string.Empty;
    public string Header  { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string DealText  { get; set; } = string.Empty;
    public string HighlightImageUrl  { get; set; } = string.Empty;
    public string TaglineIcon { get; set; } = "icon-flame";
}
