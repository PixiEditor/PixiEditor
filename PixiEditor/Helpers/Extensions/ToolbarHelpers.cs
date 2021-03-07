using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers.Extensions
{
    public static class ToolbarHelpers
    {
        public static EnumSetting<TEnum> GetEnumSetting<TEnum>(this Toolbar toolbar, string name)
            where TEnum : struct, Enum
        {
            return toolbar.GetSetting<EnumSetting<TEnum>>(name);
        }
    }
}