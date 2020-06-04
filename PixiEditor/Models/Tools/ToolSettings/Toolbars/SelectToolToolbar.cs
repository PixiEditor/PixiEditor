using PixiEditor.Models.Tools.ToolSettings.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditor.Models.Tools.ToolSettings.Toolbars
{
    public class SelectToolToolbar : Toolbar
    {
        public SelectToolToolbar()
        {
            Settings.Add(new DropdownSetting("Mode", new string[] {"New"}, "Selection type"));
        }
    }
}
