using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Containers.Toolbars;

internal interface IToolbar : IHandler
{
    public Setting GetSetting(string name);
}
