using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.UserPreferences;

namespace PixiEditor.ViewModels.UserPreferences.Settings;

internal class GeneralSettings : SettingsGroup
{
    private LanguageData? selectedLanguage = ILocalizationProvider.Current?.SelectedLanguage;
    private List<LanguageData>? availableLanguages = ILocalizationProvider.Current?.LocalizationData.Languages
        .OrderByDescending(x => x == ILocalizationProvider.Current.FollowSystem)
        .ThenByDescending(x => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == x.Code || CultureInfo.InstalledUICulture.TwoLetterISOLanguageName == x.Code)
        .ThenBy(x => x.Name).ToList();

    private bool isDebugModeEnabled = GetPreference(nameof(IsDebugModeEnabled), false);
    public bool IsDebugModeEnabled
    {
        get => isDebugModeEnabled;
        set => RaiseAndUpdatePreference(ref isDebugModeEnabled, value);
    }
    
    public List<LanguageData>? AvailableLanguages
    {
        get => availableLanguages;
        set => SetProperty(ref availableLanguages, value);
    }

    public LanguageData? SelectedLanguage
    {
        get => selectedLanguage;
        set
        {
            if (SetProperty(ref selectedLanguage, value))
            {
                ILocalizationProvider.Current?.LoadLanguage(value);
                IPreferences.Current.UpdatePreference("LanguageCode", value.Code);
            }
        }
    }

    private bool isAnalyticsEnabled = GetPreference(PreferencesConstants.AnalyticsEnabled, PreferencesConstants.AnalyticsEnabledDefault);
    public bool AnalyticsEnabled
    {
        get => isAnalyticsEnabled;
        set => RaiseAndUpdatePreference(ref isAnalyticsEnabled, value);
    }
}
