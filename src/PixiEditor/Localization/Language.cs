using System.Diagnostics;

namespace PixiEditor.Localization;

[DebuggerDisplay("{LanguageData.Name}, strings: {Locale.Count}")]
public class Language
{
    public LanguageData LanguageData { get; }
    public IReadOnlyDictionary<string, string> Locale { get; }
    
    public Language(LanguageData languageData, Dictionary<string, string> locale)
    {
        LanguageData = languageData;
        Locale = locale;
    }
}
