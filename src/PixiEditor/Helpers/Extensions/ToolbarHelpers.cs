using System;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Helpers.Extensions;

public static class ToolbarHelpers
{
    public static EnumSetting<TEnum> GetEnumSetting<TEnum>(this Toolbar toolbar, string name)
        where TEnum : struct, Enum
    {
        return toolbar.GetSetting<EnumSetting<TEnum>>(name);
    }
}