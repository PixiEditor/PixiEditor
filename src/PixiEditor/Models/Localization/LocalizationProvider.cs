using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using PixiEditor.Models.IO;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.Models.Localization;

internal class LocalizationProvider : ILocalizationProvider
{
    private Language debugLanguage;
    public string LocalizationDataPath { get; } = Path.Combine(Paths.DataFullPath, "Localization", "LocalizationData.json");
    public LocalizationData LocalizationData { get; private set; }
    public Language CurrentLanguage { get; set; }
    public LanguageData SelectedLanguage { get; private set; }
    public LanguageData FollowSystem { get; } = new() { Name = "Follow system", Code = "system" };
    public event Action<Language> OnLanguageChanged;
    public void ReloadLanguage() => OnLanguageChanged?.Invoke(CurrentLanguage);

    public Language DefaultLanguage { get; private set; }

    public void LoadData()
    {
        Newtonsoft.Json.JsonSerializer serializer = new();
        
        if (!File.Exists(LocalizationDataPath))
        {
            throw new FileNotFoundException("Localization data file not found.", LocalizationDataPath);
        }
        
        using StreamReader reader = new(LocalizationDataPath);
        LocalizationData = serializer.Deserialize<LocalizationData>(new JsonTextReader(reader) { Culture = CultureInfo.InvariantCulture, DateTimeZoneHandling = DateTimeZoneHandling.Utc });
            
        if (LocalizationData is null)
        {
            throw new InvalidDataException("Localization data is null.");
        }
        
        if (LocalizationData.Languages is null || LocalizationData.Languages.Count == 0)
        {
            throw new InvalidDataException("Localization data does not contain any languages.");
        }

        LocalizationData.Languages.Add(FollowSystem);
        
        DefaultLanguage = LoadLanguageInternal(LocalizationData.Languages[0]);
        
        string currentLanguageCode = IPreferences.Current.GetPreference<string>("LanguageCode");

        LoadLanguage(LocalizationData.Languages.FirstOrDefault(x => x.Code == currentLanguageCode, FollowSystem));
    }

    public void LoadLanguage(LanguageData languageData)
    {
        if (languageData is null)
        {
            throw new ArgumentNullException(nameof(languageData));
        }
        
        if(languageData.Code == CurrentLanguage?.LanguageData.Code)
        {
            return;
        }
        
        bool firstLoad = CurrentLanguage is null;
        
        SelectedLanguage = languageData;

        if (languageData.Code == FollowSystem.Code)
        {
            string osLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            languageData = LocalizationData.Languages.FirstOrDefault(x => x.Code == osLanguage, LocalizationData.Languages[0]);
        }
        
        CurrentLanguage = LoadLanguageInternal(languageData);

        if (!firstLoad)
        {
            OnLanguageChanged?.Invoke(CurrentLanguage);
        }
    }

    public void LoadDebugKeys(Dictionary<string, string> languageKeys, bool rightToLeft)
    {
        debugLanguage = new Language(
            new LanguageData
        {
            Code = "debug",
            Name = "Debug"
        }, languageKeys, rightToLeft);

        CurrentLanguage = debugLanguage;
        
        OnLanguageChanged?.Invoke(debugLanguage);
    }

    private Language LoadLanguageInternal(LanguageData languageData)
    {
        string localePath = Path.Combine(Paths.DataFullPath, "Localization", "Languages", languageData.LocaleFileName);

        if (!File.Exists(localePath))
        {
            throw new FileNotFoundException("Locale file not found.", localePath);
        }

        Newtonsoft.Json.JsonSerializer serializer = new();
        using StreamReader reader = new(localePath);
        Dictionary<string, string> locale =
            serializer.Deserialize<Dictionary<string, string>>(new Newtonsoft.Json.JsonTextReader(reader));

        if (locale is null)
        {
            throw new InvalidDataException("Locale is null.");
        }

        return new(languageData, locale, languageData.RightToLeft);
    }
}
