using System.Text.Json.Serialization;

namespace PixiEditor.UpdateModule;

public class ReleaseInfo
{
    public ReleaseInfo()
    {
    }

    public ReleaseInfo(bool dataFetchSuccessful)
    {
        WasDataFetchSuccessful = dataFetchSuccessful;
    }

    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }

    [JsonPropertyName("draft")]
    public bool IsDraft { get; set; }

    [JsonPropertyName("prerelease")]
    public bool IsPrerelease { get; set; }

    [JsonPropertyName("assets")]
    public Asset[] Assets { get; set; }

    public bool WasDataFetchSuccessful { get; set; } = true;
}