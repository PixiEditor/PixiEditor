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

        public SettingsViewModel(SettingsWindowViewModel owner)
            : base(owner)
        {
        }
    }
}