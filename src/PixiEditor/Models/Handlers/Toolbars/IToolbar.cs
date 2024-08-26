using System.Collections.ObjectModel;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IToolbar : IHandler
{
    public Setting GetSetting(string name);
    public ObservableCollection<Setting> Settings { get; set; }
    public void SaveToolbarSettings();
    public void LoadSharedSettings();
}
