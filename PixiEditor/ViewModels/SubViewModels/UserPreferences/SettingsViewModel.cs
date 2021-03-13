using System;
using System.Configuration;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences
{
    public class SettingsViewModel : SubViewModel<SettingsWindowViewModel>
    {
        public FileSettings File { get; set; } = new FileSettings();

        public UpdateSettings Update { get; set; } = new UpdateSettings();

        public DiscordSettings Discord { get; set; } = new DiscordSettings();

        public SettingsViewModel(SettingsWindowViewModel owner)
            : base(owner)
        {
        }
    }
}