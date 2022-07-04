using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings;

public class ToolsSettings : SettingsGroup
{
    private bool enableSharedToolbar = GetPreference(nameof(EnableSharedToolbar), false);

    public bool EnableSharedToolbar
    {
        get => enableSharedToolbar;
        set
        {
            enableSharedToolbar = value;
            RaiseAndUpdatePreference(nameof(EnableSharedToolbar), value);
        }
    }
}