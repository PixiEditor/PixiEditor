using Drawie.Backend.Core.Text;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

internal class TextToolbar : FillableShapeToolbar, ITextToolbar
{
    public FontFamilyName FontFamily
    {
        get
        {
            return GetSetting<FontFamilySettingViewModel>(nameof(FontFamily)).Value;
        }
        set
        {
            GetSetting<FontFamilySettingViewModel>(nameof(FontFamily)).Value = value;
        }
    }
    
    public double FontSize
    {
        get
        {
            return GetSetting<SizeSettingViewModel>(nameof(FontSize)).Value;
        }
        set
        {
            GetSetting<SizeSettingViewModel>(nameof(FontSize)).Value = value;
        }
    }
    
    public TextToolbar()
    {
        AddSetting(new FontFamilySettingViewModel(nameof(FontFamily), "FONT_LABEL"));
        var sizeSetting = new SizeSettingViewModel(nameof(FontSize), "FONT_SIZE_LABEL") { Value = 12 };
        AddSetting(sizeSetting);
    }
}
