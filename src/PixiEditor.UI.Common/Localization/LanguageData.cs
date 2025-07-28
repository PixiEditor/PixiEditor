using System.Globalization;
using Newtonsoft.Json;

namespace PixiEditor.UI.Common.Localization;

public class LanguageData
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string LocaleFileName { get; set; }
    
    // https://icons8.com/icon/set/flags/color
    public string IconFileName { get; set; }
    public string IconPath = $"/Images/LanguageFlags/";

    [JsonIgnore]
    public List<string> AdditionalLocalePaths { get; set; }

    [JsonIgnore]
    public string IconFullPath => $"{IconPath}{IconFileName}";
    public bool RightToLeft { get; set; }
    
    [JsonIgnore]
    public DateTimeOffset LastUpdated => LastUpdatedString == null ? DateTimeOffset.MinValue : DateTimeOffset.Parse(LastUpdatedString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    
    [JsonProperty(nameof(LastUpdated))]
    private string LastUpdatedString { get; set; }

    [JsonIgnore]
    public string? CustomLocaleAssemblyPath { get; set; }
    
    public override string ToString()
    {
        return Name;
    }
}
