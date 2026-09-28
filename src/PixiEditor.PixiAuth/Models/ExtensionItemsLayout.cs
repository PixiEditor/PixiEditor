namespace PixiEditor.PixiAuth.Models;

[Serializable]
public class ExtensionItemsLayout
{
    public List<ExtensionHighlight> HighlightedExtensions { get; set; } = new List<ExtensionHighlight>();
    public List<string> FeaturedExtensionIds { get; set; } = new List<string>();
}

[Serializable]
public class ExtensionHighlight
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
