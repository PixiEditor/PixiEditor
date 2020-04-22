using PixiEditor.Models.Tools.ToolSettings.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public class BasicToolbar : Toolbar
    {
        public BasicToolbar()
        {
            Settings.Add(new SizeSetting("ToolSize", "Tool size:"));
        }
    }
}
