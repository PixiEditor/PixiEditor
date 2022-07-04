using PixiEditor.ViewModels.SubViewModels.Main;
using System.Collections.Generic;
using System.Linq;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings;

public class UpdateSettings : SettingsGroup
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

    private string updateChannelName = GetPreference("UpdateChannel", "Release");

    public string UpdateChannelName
    {
        get => updateChannelName;
        set
        {
            updateChannelName = value;
            RaiseAndUpdatePreference("UpdateChannel", value);
        }
    }

    public IEnumerable<string> UpdateChannels
    {
        get => ViewModelMain.Current.UpdateSubViewModel.UpdateChannels.Select(x => x.Name);
    }
}