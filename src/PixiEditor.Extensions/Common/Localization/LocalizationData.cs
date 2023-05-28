using System.Diagnostics;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.Models.Localization;

[DebuggerDisplay("{Languages.Count} Language(s)")]
public class LocalizationData
{
    public List<LanguageData> Languages { get; set; }
}
