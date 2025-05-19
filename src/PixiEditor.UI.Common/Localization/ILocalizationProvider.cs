namespace PixiEditor.UI.Common.Localization;

public interface ILocalizationProvider
{
    public static ILocalizationProvider? Current { get; private set; }
    public string LocalizationDataPath { get; }
    public LocalizationData LocalizationData { get; }
    public Language CurrentLanguage { get; set; }
    public LanguageData SelectedLanguage { get; }
    public LanguageData FollowSystem { get; }
    public event Action<Language> OnLanguageChanged;
    public static event Action<ILocalizationProvider> OnLocalizationProviderChanged;

    /// <summary>
    ///     Loads the localization data from the specified file.
    /// </summary>
    public void LoadData(string currentLanguageCode = null);
    public void LoadLanguage(LanguageData languageData, bool forceReload = false);
    public void LoadExtensionData(Extension extension);
    public void LoadDebugKeys(Dictionary<string, string> languageKeys, bool rightToLeft);
    public void ReloadLanguage();
    public Language DefaultLanguage { get; }

    protected static void SetAsCurrent(ILocalizationProvider provider)
    {
        Current = provider;
        OnLocalizationProviderChanged?.Invoke(provider);
    }
}
