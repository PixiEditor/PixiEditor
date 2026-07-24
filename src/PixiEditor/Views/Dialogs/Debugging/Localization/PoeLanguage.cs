using System.Globalization;
using System.Text.Json.Serialization;
using Avalonia.Media;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Views.Dialogs.Debugging.Localization;

public class PoeLanguage
{
    private static readonly SolidColorBrush LocalOlder = new(Colors.Red);
    private static readonly SolidColorBrush LocalMin = new(Colors.Orange);
    private static readonly SolidColorBrush Equal = new(Colors.Lime);
    private static readonly SolidColorBrush LocalNewer = new(Colors.DodgerBlue);
    private static readonly SolidColorBrush LocalMissing = new(Colors.Gray);

    public string Name { get; set; }

    public string Code { get; set; }

    [JsonPropertyName("Updated")] public string UpdatedText { get; set; }

    [JsonIgnore]
    public DateTimeOffset LastUpdatedUTC => string.IsNullOrWhiteSpace(UpdatedText)
        ? DateTimeOffset.MinValue
        : DateTimeOffset.Parse(UpdatedText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

    public double Percentage { get; set; }

    public DateTimeOffset LastUpdatedLocalTime => LastUpdatedUTC.ToLocalTime();

    public bool IsRightToLeft => Code is "ar" or "he" or "ku" or "fa" or "ur";

    public LanguageData BuiltinEquivalent { get; set; }

    public SolidColorBrush StatusBrush => Status switch
    {
        LanguageStatus.LocalMissing => LocalMissing,
        LanguageStatus.LocalMin => LocalMin,
        LanguageStatus.LocalOlder => LocalOlder,
        LanguageStatus.Equal => Equal,
        LanguageStatus.LocalNewer => LocalNewer,
        _ => throw new ArgumentOutOfRangeException()
    };

    public string StatusText => Status switch
    {
        LanguageStatus.LocalMissing => $"PixiEditor has no built-in {Name} language",
        LanguageStatus.LocalMin => $"The last update date for the built-in {Name} language is unknown",
        LanguageStatus.LocalOlder => $"POEditor has new changes not yet incorporated into PixiEditor",
        LanguageStatus.Equal => $"Built-in data for {Name} is up-to-date with POEditor",
        LanguageStatus.LocalNewer => $"POEditor data for this language is outdated"
    };

    public string DownloadLanguageAndApplyToEditorText => $"Download {Name} language data and apply to editor";

    public string UpdateLanguageJsonInSourceCodeText =>
        $"Download {Name} language data and write it into {Code}.json in the project";

    private LanguageStatus Status
    {
        get
        {
            if (Code == "en")
                return LanguageStatus.Equal;
            
            return (l: BuiltinEquivalent?.LastUpdatedUTC, r: LastUpdatedUTC) switch
            {
                (null, _) => LanguageStatus.LocalMissing,
                { l.Ticks: 0 } => LanguageStatus.LocalMin,
                { l: var l, r: var r } when r > l.Value => LanguageStatus.LocalOlder,
                { l: var l, r: var r } when r == l.Value => LanguageStatus.Equal,
                { l: var l, r: var r } when r < l.Value => LanguageStatus.LocalNewer,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public override string ToString() => Name;

    enum LanguageStatus
    {
        LocalMissing,
        LocalMin,
        LocalOlder,
        Equal,
        LocalNewer
    }
}
