using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings
{
    public class GeneralSettings : SettingsGroup
    {
        private bool imagePreviewInTaskbar = GetPreference(nameof(ImagePreviewInTaskbar), false);

        public bool ImagePreviewInTaskbar
        {
            get => imagePreviewInTaskbar;
            set => RaiseAndUpdatePreference(ref imagePreviewInTaskbar, value);
        }
    }
}
