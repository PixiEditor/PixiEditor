using PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences
{
    public class SettingsViewModel : SubViewModel<SettingsWindowViewModel>
    {
        public GeneralSettings General { get; set; } = new GeneralSettings();

        public ToolsSettings Tools { get; set; } = new ToolsSettings();

        public FileSettings File { get; set; } = new FileSettings();

        public UpdateSettings Update { get; set; } = new UpdateSettings();

        public DiscordSettings Discord { get; set; } = new DiscordSettings();

        public SettingsViewModel(SettingsWindowViewModel owner)
            : base(owner)
        {
        }
    }
}