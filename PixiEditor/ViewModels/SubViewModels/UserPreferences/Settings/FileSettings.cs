using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings
{
    public class FileSettings : SettingsGroup
    {
        private bool showNewFilePopupOnStartup = GetPreference("ShowNewFilePopupOnStartup", true);

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

        private int maxOpenedRecently = GetPreference(nameof(MaxOpenedRecently), 10);

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