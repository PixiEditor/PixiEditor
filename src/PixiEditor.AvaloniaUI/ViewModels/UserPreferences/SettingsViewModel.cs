using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.AvaloniaUI.ViewModels.UserPreferences.Settings;

namespace PixiEditor.AvaloniaUI.ViewModels.UserPreferences;

internal class SettingsViewModel : SubViewModel<SettingsWindowViewModel>
{
    public GeneralSettings General { get; set; } = new();

    public ToolsSettings Tools { get; set; } = new();

    public FileSettings File { get; set; } = new();

    public UpdateSettings Update { get; set; } = new();

    public DiscordSettings Discord { get; set; } = new();

    public SettingsViewModel(SettingsWindowViewModel owner)
        : base(owner)
    {
    }
}
