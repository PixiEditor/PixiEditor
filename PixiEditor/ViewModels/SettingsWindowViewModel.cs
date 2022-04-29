using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Dialogs;
using PixiEditor.ViewModels.SubViewModels.UserPreferences;
using System.Collections.ObjectModel;

namespace PixiEditor.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        public bool ShowUpdateTab
        {
            get
            {
#if UPDATE || DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public SettingsViewModel SettingsSubViewModel { get; set; }

        public ObservableCollection<CommandGroup> Commands { get; }

        [Models.Commands.Attributes.Command.Internal("PixiEditor.Shortcuts.Reset")]
        public static void ResetCommand()
        {
            var dialog = new OptionsDialog<string>("Are you sure?", "Are you sure you want to reset all shortcuts to their default value?")
            {
                { "Yes", x => CommandController.Current.ResetShortcuts() },
                "Cancel"
            }.ShowDialog();
        }

        public SettingsWindowViewModel()
        {
            Commands = new(CommandController.Current.CommandGroups);
            SettingsSubViewModel = new SettingsViewModel(this);
        }
    }
}
