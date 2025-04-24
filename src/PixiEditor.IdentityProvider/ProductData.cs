namespace PixiEditor.IdentityProvider;

public record ProductData
{
    public string Id { get; set; }
    public string DisplayName { get; set; }

    public ProductData(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }
}
