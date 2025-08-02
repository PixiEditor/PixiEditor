using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Drawie.Backend.Core.Bridge;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.ViewModels.UserPreferences.Settings;

internal class GeneralSettings : SettingsGroup
{
    private string? selectedRenderApi = RenderApiPreferenceManager.TryReadRenderApiPreference();

    private List<string>? availableRenderApis = IOperatingSystem.Current?.GetAvailableRenderers()?.ToList() ??
                                                new List<string>();

    private LanguageData? selectedLanguage = ILocalizationProvider.Current?.SelectedLanguage;

    private List<LanguageData>? availableLanguages = ILocalizationProvider.Current?.LocalizationData.Languages
        .OrderByDescending(x => x == ILocalizationProvider.Current.FollowSystem)
        .ThenByDescending(x =>
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == x.Code ||
            CultureInfo.InstalledUICulture.TwoLetterISOLanguageName == x.Code)
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

    private bool isAnalyticsEnabled =
        GetPreference(PreferencesConstants.AnalyticsEnabled, PreferencesConstants.AnalyticsEnabledDefault);

    public bool AnalyticsEnabled
    {
        get => isAnalyticsEnabled;
        set => RaiseAndUpdatePreference(ref isAnalyticsEnabled, value);
    }

    public List<string> AvailableRenderApis
    {
        get
        {
            if (availableRenderApis == null || availableRenderApis.Count == 0)
            {
                availableRenderApis = new List<string>(IOperatingSystem.Current?.GetAvailableRenderers() ?? []);
            }

            return availableRenderApis;
        }
    }

    public string SelectedRenderApi
    {
        get => selectedRenderApi ?? AvailableRenderApis.FirstOrDefault() ?? string.Empty;
        set
        {
            if (SetProperty(ref selectedRenderApi, value))
            {
                try
                {
                    RenderApiPreferenceManager.UpdateRenderApiPreference(value);
                    OnPropertyChanged(nameof(RenderApiChangePending));
                }
                catch (Exception ex)
                {
                    NoticeDialog.Show(
                        new LocalizedString("ERROR_SAVING_PREFERENCES_DESC", ex.Message),
                        "ERROR_SAVING_PREFERENCES");
                }
            }
        }
    }

    public bool RenderApiChangePending =>
        selectedRenderApi != RenderApiPreferenceManager.FirstReadApiPreference;
}
