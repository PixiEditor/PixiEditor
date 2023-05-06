using System.Windows.Media;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
#nullable enable
internal class BasicShapeToolbar : BasicToolbar
{
    public bool Fill => GetSetting<BoolSetting>(nameof(Fill)).Value;
    public Color FillColor => GetSetting<ColorSetting>(nameof(FillColor)).Value;

    public BasicShapeToolbar()
    {
        Settings.Add(new BoolSetting(nameof(Fill), "FILL_SHAPE_LABEL"));
        Settings.Add(new ColorSetting(nameof(FillColor), "FILL_COLOR_LABEL"));
    }
}
