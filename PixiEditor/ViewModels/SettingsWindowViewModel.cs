using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Helpers;
using PixiEditor.ViewModels.SubViewModels.UserPreferences;

namespace PixiEditor.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        public bool ShowUpdateTab
        {
            get
            {
#if UPDATE || DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public SettingsViewModel SettingsSubViewModel { get; set; }

        public SettingsWindowViewModel()
        {
            SettingsSubViewModel = new SettingsViewModel(this);
        }
    }
}
