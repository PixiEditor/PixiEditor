using System.Text.Json.Serialization;

namespace PixiEditor.UpdateModule;

public class Asset
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }
}