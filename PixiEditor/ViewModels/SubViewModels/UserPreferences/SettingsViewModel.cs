using System;
using System.Configuration;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences
{
    public class SettingsViewModel : SubViewModel<SettingsWindowViewModel>
    {
        private bool showNewFilePopupOnStartup = PreferencesSettings.GetPreference("ShowNewFilePopupOnStartup", true);

        public bool ShowNewFilePopupOnStartup
        {
            get => showNewFilePopupOnStartup;
            set
            {
                showNewFilePopupOnStartup = value;
                string name = nameof(ShowNewFilePopupOnStartup);
                RaisePropertyChanged(name);
                PreferencesSettings.UpdatePreference(name, value);
            }
        }

        private bool checkUpdatesOnStartup = PreferencesSettings.GetPreference("CheckUpdatesOnStartup", true);

        public bool CheckUpdatesOnStartup
        {
            get => checkUpdatesOnStartup;
            set
            {
                checkUpdatesOnStartup = value;
                string name = nameof(CheckUpdatesOnStartup);
                RaisePropertyChanged(name);
                PreferencesSettings.UpdatePreference(name, value);
            }
        }


        public SettingsViewModel(SettingsWindowViewModel owner)
            : base(owner)
        {
        }
    }
}