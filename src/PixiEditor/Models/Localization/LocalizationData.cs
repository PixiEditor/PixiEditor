using System.Diagnostics;

namespace PixiEditor.Models.Localization;

[DebuggerDisplay("{Languages.Count} Language(s)")]
public class LocalizationData
{
    public List<LanguageData> Languages { get; set; }
}
