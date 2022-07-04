using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.ToolSettings.Toolbars
{
    public class PenToolbar : BasicToolbar
    {
        public PenToolbar()
        {
            Settings.Add(new BoolSetting("PixelPerfectEnabled", "Pixel perfect"));
        }
    }
}