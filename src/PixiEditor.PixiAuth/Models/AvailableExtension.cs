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

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("price")]
    public int Price { get; set; }
    
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    
    [JsonPropertyName("includedExtensions")]
    public List<string> IncludedExtensions { get; set; } = new List<string>();

    [JsonPropertyName("body")]
    public string Body { get; set; }

    [JsonPropertyName("showcaseUrls")]
    public string[] ShowcaseUrls { get; set; }

    [JsonPropertyName("isBundle")]
    public bool IsBundle { get; set; }

    [JsonPropertyName("percentageDiscount")]
    public int PercentageDiscount { get; set; }

    [JsonPropertyName("releaseDate")]
    public DateTime ReleaseDate { get; set; }

    public List<ExtVersion> Versions { get; set; } = new List<ExtVersion>();
}

[Serializable]
public class ExtensionHighlightData
{
    [JsonPropertyName("headerTaglineText")]
    public string HeaderTaglineText { get; set; } = string.Empty;

    [JsonPropertyName("header")]
    public string Header { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("dealText")]
    public string DealText { get; set; } = string.Empty;

    [JsonPropertyName("highlightImageUrl")]
    public string HighlightImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; }  = string.Empty;

    [JsonPropertyName("taglineIcon")]
    public string TaglineIcon { get; set; }
}

[Serializable]
public class ExtVersion
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("pixiEditorApiVersion")]
    public int PixiEditorApiVersion { get; set; }
}

