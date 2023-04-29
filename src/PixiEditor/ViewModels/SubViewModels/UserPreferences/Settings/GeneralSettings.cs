using System.Globalization;
using PixiEditor.Localization;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings;

internal class GeneralSettings : SettingsGroup
{
    private bool imagePreviewInTaskbar = GetPreference(nameof(ImagePreviewInTaskbar), false);
    private LanguageData selectedLanguage = ILocalizationProvider.Current.SelectedLanguage;
    private List<LanguageData> availableLanguages = ILocalizationProvider.Current.LocalizationData.Languages
        .OrderByDescending(x => x == ILocalizationProvider.Current.FollowSystem)
        .ThenByDescending(x => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == x.Code || CultureInfo.InstalledUICulture.TwoLetterISOLanguageName == x.Code)
        .ThenBy(x => x.Name).ToList();

    public bool ImagePreviewInTaskbar
    {
        get => imagePreviewInTaskbar;
        set => RaiseAndUpdatePreference(ref imagePreviewInTaskbar, value);
    }

    private bool isDebugModeEnabled = GetPreference(nameof(IsDebugModeEnabled), false);
    public bool IsDebugModeEnabled
    {
        get => isDebugModeEnabled;
        set => RaiseAndUpdatePreference(ref isDebugModeEnabled, value);
    }
    
    public List<LanguageData> AvailableLanguages
    {
        get => availableLanguages;
        set => SetProperty(ref availableLanguages, value);
    }

    public LanguageData SelectedLanguage
    {
        get => selectedLanguage;
        set
        {
            if (SetProperty(ref selectedLanguage, value))
            {
                ILocalizationProvider.Current.LoadLanguage(value);
                IPreferences.Current.UpdatePreference("LanguageCode", value.Code);
            }
        }
    }
}
