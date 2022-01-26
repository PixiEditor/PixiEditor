using PixiEditor.Models;
using PixiEditor.Models.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings
{
    public class FileSettings : SettingsGroup
    {
        private bool showStartupWindow = GetPreference(nameof(ShowStartupWindow), true);

        public bool ShowStartupWindow
        {
            get => showStartupWindow;
            set => RaiseAndUpdatePreference(ref showStartupWindow, value);
        }

        private int defaultNewFileWidth = GetPreference("DefaultNewFileWidth", Constants.DefaultSize);

        public int DefaultNewFileWidth
        {
            get => defaultNewFileWidth;
            set
            {
                defaultNewFileWidth = value;
                string name = nameof(DefaultNewFileWidth);
                RaiseAndUpdatePreference(name, value);
            }
        }

        private int defaultNewFileHeight = GetPreference("DefaultNewFileHeight", Constants.DefaultSize);

        public int DefaultNewFileHeight
        {
            get => defaultNewFileHeight;
            set
            {
                defaultNewFileHeight = value;
                string name = nameof(DefaultNewFileHeight);
                RaiseAndUpdatePreference(name, value);
            }
        }

        private int maxOpenedRecently = GetPreference(nameof(MaxOpenedRecently), 8);

        public int MaxOpenedRecently
        {
            get => maxOpenedRecently;
            set => RaiseAndUpdatePreference(ref maxOpenedRecently, value);
        }
    }
}