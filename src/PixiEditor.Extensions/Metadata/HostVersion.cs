namespace PixiEditor.Extensions.Metadata;

[Serializable]
public class HostVersion
{
    public string HostName { get; set; } = string.Empty;
    public Version? MinVersion { get; set; }
    public Version? MaxVersion { get; set; }
}
