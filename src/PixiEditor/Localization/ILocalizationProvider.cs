using System.IO;

namespace PixiEditor.Localization;

public interface ILocalizationProvider
{
    public static ILocalizationProvider Current => ViewModelMain.Current.LocalizationProvider;
    
    public string LocalizationDataPath { get; }
    public LocalizationData LocalizationData { get; }
    public Language CurrentLanguage { get; set; }
    public LanguageData SelectedLanguage { get; }
    public LanguageData FollowSystem { get; }
    public event Action<Language> OnLanguageChanged;

    /// <summary>
    ///     Loads the localization data from the specified file.
    /// </summary>
    public void LoadData();
    public void LoadLanguage(LanguageData languageData);
    public void LoadDebugKeys(Dictionary<string, string> languageKeys, bool rightToLeft);
    public void ReloadLanguage();
    public Language DefaultLanguage { get; }
}
