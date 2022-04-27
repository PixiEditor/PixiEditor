using PixiEditor.Models.Commands;
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

        public ObservableCollection<Command> Commands { get; }

        public SettingsWindowViewModel()
        {
            Commands = new(CommandController.Current.Commands.Where(x => !string.IsNullOrWhiteSpace(x.Display)));
            SettingsSubViewModel = new SettingsViewModel(this);
        }
    }
}
