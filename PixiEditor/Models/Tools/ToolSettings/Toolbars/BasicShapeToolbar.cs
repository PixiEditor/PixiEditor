using PixiEditor.Models.Tools.ToolSettings.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public class BasicShapeToolbar : BasicToolbar
    {
        public BasicShapeToolbar()
        {
            Settings.Add(new BoolSetting("Fill", "Fill shape: "));
            Settings.Add(new ColorSetting("FillColor", "Fill color"));            
        }
    }
}
