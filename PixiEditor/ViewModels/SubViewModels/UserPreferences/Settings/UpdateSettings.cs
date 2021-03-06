namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings
{
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
    }
}