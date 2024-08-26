using System.Collections.Generic;
using System.Linq;

namespace PixiEditor.ViewModels.UserPreferences.Settings;

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

    private string updateChannelName =
#if UPDATE
        GetPreference("UpdateChannel", "Release");
#else
        ViewModelMain.Current?.UpdateSubViewModel?.UpdateChannels?.FirstOrDefault()?.Name ?? "Unknown";
#endif

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
