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
                RaiseAndUpdatePreference(name, value);
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
                RaiseAndUpdatePreference(name, value);
            }
        }

        private long defaultNewFileWidth = (int)PreferencesSettings.GetPreference("DefaultNewFileWidth", 16L);

        public long DefaultNewFileWidth
        {
            get => defaultNewFileWidth;
            set
            {
                defaultNewFileWidth = value;
                string name = nameof(DefaultNewFileWidth);
                RaiseAndUpdatePreference(name, value);
            }
        }

        private long defaultNewFileHeight = (int)PreferencesSettings.GetPreference("DefaultNewFileHeight", 16L);

        public long DefaultNewFileHeight
        {
            get => defaultNewFileHeight;
            set
            {
                defaultNewFileHeight = value;
                string name = nameof(DefaultNewFileHeight);
                RaiseAndUpdatePreference(name, value);
            }
        }

        private bool enableRichPresence = PreferencesSettings.GetPreference<bool>(nameof(EnableRichPresence));

        public bool EnableRichPresence
        {
            get => enableRichPresence;
            set
            {
                enableRichPresence = value;
                RaiseAndUpdatePreference(nameof(EnableRichPresence), value);
            }
        }

        public void RaiseAndUpdatePreference<T>(string name, T value)
        {
            RaisePropertyChanged(name);
            PreferencesSettings.UpdatePreference(name, value);
        }

        public SettingsViewModel(SettingsWindowViewModel owner)
            : base(owner)
        {
        }
    }
}