namespace PixiEditor.IdentityProvider;

public record ProductData
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string ImageUrl { get; set; }
    public string? LatestVersion { get; set; }
    public string DownloadLink { get; set; }

    public ProductData(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }
}
