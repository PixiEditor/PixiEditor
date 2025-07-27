using System.Text.Json.Serialization;

namespace PixiEditor.PixiAuth.Models;

[Serializable]
public class Product
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("isDlc")]
    public bool IsDlc { get; set; }

    [JsonPropertyName("target")]
    public string Target { get; set; }

    [JsonPropertyName("latestVersion")]
    public string? LatestVersion { get; set; }
}
