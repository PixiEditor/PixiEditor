using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.UserPreferences.Settings;

namespace PixiEditor.ViewModels.UserPreferences;

internal class SettingsViewModel : SubViewModel<SettingsWindowViewModel>
{
    public GeneralSettings General { get; set; } = new();

    public ToolsSettings Tools { get; set; } = new();

    public FileSettings File { get; set; } = new();

    public UpdateSettings Update { get; set; } = new();

    public DiscordSettings Discord { get; set; } = new();

    public SceneSettings Scene { get; set; } = new();

    public SettingsViewModel(SettingsWindowViewModel owner)
        : base(owner)
    {
    }
}
