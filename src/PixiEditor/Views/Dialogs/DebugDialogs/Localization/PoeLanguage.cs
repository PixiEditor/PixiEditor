using System.Globalization;
using System.Windows.Media;
using Newtonsoft.Json;
using PixiEditor.Localization;

namespace PixiEditor.Views.Dialogs.DebugDialogs.Localization;

public class PoeLanguage
    {
        private static readonly SolidColorBrush LocalOlder = new(Colors.Red);
        private static readonly SolidColorBrush LocalMin = new(Colors.Orange);
        private static readonly SolidColorBrush Equal = new(Colors.Lime);
        private static readonly SolidColorBrush LocalNewer = new(Colors.DodgerBlue);
        private static readonly SolidColorBrush LocalMissing = new(Colors.Gray);

        public string Name { get; set; }

        public string Code { get; set; }

        [JsonProperty("Updated")]
        public string UpdatedText { get; set; }

        [JsonIgnore]
        public DateTimeOffset UpdatedUTC => string.IsNullOrWhiteSpace(UpdatedText)
            ? DateTimeOffset.MinValue
            : DateTimeOffset.Parse(UpdatedText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        
        public double Percentage { get; set; }

        public DateTimeOffset UpdatedLocal => UpdatedUTC.ToLocalTime();

        public bool IsRightToLeft => Code is "ar" or "he" or "ku" or "fa" or "ur";

        public LanguageData LocalEquivalent { get; set; }

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
            LanguageStatus.LocalMissing or LanguageStatus.LocalMin => "Source missing/unset",
            LanguageStatus.LocalOlder => "Source older",
            LanguageStatus.Equal => "Source up to date",
            LanguageStatus.LocalNewer => "Source newer"
        };

        private LanguageStatus Status => (l: LocalEquivalent?.LastUpdated, r: UpdatedUTC) switch
        {
            (null, _) => LanguageStatus.LocalMissing,
            { l.Ticks: 0 } => LanguageStatus.LocalMin,
            { l: var l, r: var r } when r > l.Value => LanguageStatus.LocalOlder,
            { l: var l, r: var r } when r == l.Value => LanguageStatus.Equal,
            { l: var l, r: var r } when r < l.Value => LanguageStatus.LocalNewer,
            _ => throw new ArgumentOutOfRangeException()
        };

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
