using PixiEditor.Models.Tools.ToolSettings.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditor.Models.Tools.ToolSettings.Toolbars
{
    public class BrightnessToolToolbar : BasicToolbar
    {
        public BrightnessToolToolbar(float initialValue)
        {
            Settings.Add(new FloatSetting("CorrectionFactor", initialValue, "Strength:", 0f, 100f));
        }
    }
}
