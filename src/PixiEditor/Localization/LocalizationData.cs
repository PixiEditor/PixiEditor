using System.Diagnostics;

namespace PixiEditor.Localization;

[DebuggerDisplay("{Languages.Length} Language(s)")]
public class LocalizationData
{
    public LanguageData[] Languages { get; set; }
}
