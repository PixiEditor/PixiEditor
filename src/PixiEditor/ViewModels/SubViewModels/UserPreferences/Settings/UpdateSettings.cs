namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings;

internal class UpdateSettings : SettingsGroup
{
    private bool checkUpdatesOnStartup = GetPreference("CheckUpdatesOnStartup", true);

    public bool CheckUpdatesOnStartup
    {
        get => checkUpdatesOnStartup;
        set
        {
            checkUpdatesOnStartup = value;
            string name = nameof(CheckUpdatesOnStartup);
            RaiseAndUpdatePreference(name, value);
        }
    }

    private string updateChannelName = GetPreference("UpdateChannel",
#if UPDATE
        "Release"
#else
                    ViewModelMain.Current?.UpdateSubViewModel?.UpdateChannels?.FirstOrDefault()?.Name ?? "Release"
#endif
        );

    public string UpdateChannelName
    {
        get => updateChannelName;
        set
        {
            updateChannelName = value;
#if UPDATE
            RaiseAndUpdatePreference("UpdateChannel", value);
#endif
        }
    }

    public IEnumerable<string> UpdateChannels
    {
        get => ViewModelMain.Current.UpdateSubViewModel.UpdateChannels.Select(x => x.Name);
    }
}
