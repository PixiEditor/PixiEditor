using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private long defaultNewFileWidth = GetPreference("DefaultNewFileWidth", 16L);

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

        private long defaultNewFileHeight = GetPreference("DefaultNewFileHeight", 16L);

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

        private int maxOpenedRecently = GetPreference(nameof(MaxOpenedRecently), 8);

        public int MaxOpenedRecently
        {
            get => maxOpenedRecently;
            set
            {
                maxOpenedRecently = value;
                RaiseAndUpdatePreference(nameof(MaxOpenedRecently), value);
            }
        }
    }
}