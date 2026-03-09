using System.Text.Json.Serialization;

namespace PixiEditor.PixiAuth.Models;

[Serializable]
public class AvailableExtension
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;
    
    [JsonPropertyName("price")]
    public int Price { get; set; }
    
    [JsonPropertyName("includedExtensions")]
    public List<string> IncludedExtensions { get; set; } = new List<string>();
}

