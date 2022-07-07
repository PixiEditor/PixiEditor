using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Helpers.Extensions;

internal static class ToolbarHelpers
{
    public static EnumSetting<TEnum> GetEnumSetting<TEnum>(this Toolbar toolbar, string name)
        where TEnum : struct, Enum
    {
        return toolbar.GetSetting<EnumSetting<TEnum>>(name);
    }
}
