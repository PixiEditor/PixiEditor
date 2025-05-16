using System.Diagnostics;

namespace PixiEditor.UI.Common.Localization;

[DebuggerDisplay("{Languages.Count} Language(s)")]
public class LocalizationData
{
    public List<LanguageData> Languages { get; set; } = new();

    public void MergeWith(List<LanguageData> toMerge, string assemblyLocation)
    {
        foreach (LanguageData language in toMerge)
        {
            LanguageData existing = Languages.Find(x => x.Code == language.Code);
            if (existing is null)
            {
                language.CustomLocaleAssemblyPath = assemblyLocation;
                Languages.Add(language);
            }
            else
            {
                existing.AdditionalLocalePaths ??= new List<string>();
                existing.AdditionalLocalePaths.Add(Path.Combine(assemblyLocation, language.LocaleFileName));
            }
        }
    }
}
