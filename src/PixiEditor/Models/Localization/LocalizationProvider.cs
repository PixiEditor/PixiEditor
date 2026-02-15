using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Platform;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Helpers;
using PixiEditor.Models.IO;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Localization;

internal class LocalizationProvider : ILocalizationProvider
{
    private Language debugLanguage;

    public string LocalizationDataPath { get; } =
        Path.Combine(Paths.DataResourceUri, "Localization", "LocalizationData.json");

    public LocalizationData LocalizationData { get; private set; }

    public Language CurrentLanguage { get; set; }

    public LanguageData SelectedLanguage { get; private set; }

    public LanguageData FollowSystem { get; } = new() { Name = "Follow system", Code = "system" };

    public event Action<Language> OnLanguageChanged;

    public void ReloadLanguage() => OnLanguageChanged?.Invoke(CurrentLanguage);

    public Language DefaultLanguage { get; private set; }

    private ExtensionLoader extensionLoader;

    public LocalizationProvider(ExtensionLoader extensionLoader)
    {
        this.extensionLoader = extensionLoader;
        ILocalizationProvider.SetAsCurrent(this);
    }

    public void LoadData(string currentLanguageCode = null)
    {
        if (!AssetLoader.Exists(new Uri(LocalizationDataPath)))
        {
            throw new FileNotFoundException("Localization data file not found.", LocalizationDataPath);
        }

        using var stream = AssetLoader.Open(new Uri(LocalizationDataPath));
        LocalizationData = JsonSerializer.Deserialize<LocalizationData>(stream, JsonOptions.CasesInsensitive);

        if (LocalizationData is null)
        {
            throw new InvalidDataException("Localization data is null.");
        }

        LoadExtensionLocalizationData(LocalizationData);

        if (LocalizationData.Languages is null || LocalizationData.Languages.Count == 0)
        {
            throw new InvalidDataException("Localization data does not contain any languages.");
        }

        LocalizationData.Languages.Add(FollowSystem);

        DefaultLanguage = LoadLanguageInternal(LocalizationData.Languages[0]);

        LoadLanguage(LocalizationData.Languages.FirstOrDefault(x => x.Code == currentLanguageCode, FollowSystem));
    }

    public void LoadExtensionData(List<LanguageData> extensionLanguageData, string dataLocation)
    {
        if (extensionLanguageData == null || extensionLanguageData.Count == 0)
        {
            return;
        }

        LoadExtensionData(extensionLanguageData, dataLocation, LocalizationData);
        LoadLanguage(CurrentLanguage.LanguageData, true);
    }

    private void LoadExtensionData(List<LanguageData> extensionLanguageData, string dataLocation, LocalizationData data)
    {
        string dirName = Path.GetDirectoryName(dataLocation);

        data.MergeWith(extensionLanguageData, dirName);
    }

    private void LoadExtensionLocalizationData(LocalizationData localizationData)
    {
        if (localizationData is null)
        {
            throw new InvalidDataException(nameof(localizationData));
        }

        if (extensionLoader?.LoadedExtensions is null)
        {
            return;
        }

        foreach (Extension extension in extensionLoader?.LoadedExtensions)
        {
            if (extension.Metadata.Localization is null)
            {
                continue;
            }

            LoadExtensionData(extension.Metadata.Localization?.Languages, extension.Location, localizationData);
        }
    }

    public void LoadLanguage(LanguageData languageData, bool forceReload = false)
    {
        if (languageData is null)
        {
            throw new ArgumentNullException(nameof(languageData));
        }

        if (languageData.Code == CurrentLanguage?.LanguageData.Code && !forceReload)
        {
            return;
        }

        bool firstLoad = CurrentLanguage is null;

        SelectedLanguage = languageData;

        if (languageData.Code == FollowSystem.Code)
        {
            string osLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            languageData =
                LocalizationData.Languages.FirstOrDefault(x => x.Code == osLanguage, LocalizationData.Languages[0]);
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
            new LanguageData { Code = "debug", Name = "Debug" }, languageKeys, rightToLeft);

        CurrentLanguage = debugLanguage;

        OnLanguageChanged?.Invoke(debugLanguage);
    }

    private Language LoadLanguageInternal(LanguageData languageData)
    {
        string mainLocalePath = GetLocalePath(languageData);

        if (!AssetLoader.Exists(new Uri(mainLocalePath)))
        {
            throw new FileNotFoundException("Locale file not found.", mainLocalePath);
        }

        Dictionary<string, string> locale = new Dictionary<string, string>();

        languageData.AdditionalLocalePaths ??= new List<string>();
        int localesCount = 1 + languageData.AdditionalLocalePaths.Count;

        string[] allLocalePaths = new string[localesCount];
        allLocalePaths[0] = mainLocalePath;
        languageData.AdditionalLocalePaths.CopyTo(allLocalePaths, 1);

        foreach (string localePath in allLocalePaths)
        {
            if (!AssetLoader.Exists(new Uri(localePath)) && !File.Exists(localePath))
            {
                continue;
            }

            locale.AddRangeOverride(ReadLocaleFile(localePath));
        }

        if (locale is null)
        {
            throw new InvalidDataException("Locale is null.");
        }

        return new(languageData, locale, languageData.RightToLeft);
    }

    private IDictionary<string, string> ReadLocaleFile(string localePath)
    {
        Stream stream = null;
        if (!localePath.StartsWith("avares://"))
        {
            if (!File.Exists(localePath))
            {
                throw new FileNotFoundException("Locale file not found.", localePath);
            }

            stream = File.OpenRead(localePath);
        }
        else
        {
            Uri uri = new Uri(localePath);
            if (!AssetLoader.Exists(uri))
            {
                throw new FileNotFoundException("Locale file not found.", localePath);
            }

            stream = AssetLoader.Open(new Uri(localePath));
        }

        var result = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
        stream.Dispose();

        return result;
    }

    private string GetLocalePath(LanguageData languageData)
    {
        if (languageData.CustomLocaleAssemblyPath is not null)
        {
            return Path.Combine(languageData.CustomLocaleAssemblyPath, languageData.LocaleFileName);
        }

        return Path.Combine(Paths.DataResourceUri, "Localization", "Languages", languageData.LocaleFileName);
    }
}
