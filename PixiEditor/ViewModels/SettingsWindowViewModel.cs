using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.ViewModels.SubViewModels.UserPreferences;
using System.Windows;

namespace PixiEditor.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        private string searchTerm;
        private int visibleGroups;

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

        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                if (SetProperty(ref searchTerm, value, out var oldValue) &&
                    !(string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(oldValue)))
                {
                    UpdateSearchResults();
                    VisibleGroups = Commands.Count(x => x.Visibility == Visibility.Visible);
                }
            }
        }

        public int VisibleGroups
        {
            get => visibleGroups;
            private set => SetProperty(ref visibleGroups, value);
        }

        public SettingsViewModel SettingsSubViewModel { get; set; }

        public List<GroupSearchResult> Commands { get; }

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
            Commands = new(CommandController.Current.CommandGroups.Select(x => new GroupSearchResult(x)));
            SettingsSubViewModel = new SettingsViewModel(this);
            VisibleGroups = Commands.Count(x => x.Visibility == Visibility.Visible);
        }

        public void UpdateSearchResults()
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                foreach (var group in Commands)
                {
                    group.Visibility = Visibility.Visible;
                    foreach (var command in group.Commands)
                    {
                        command.Visibility = Visibility.Visible;
                    }
                }
                return;
            }

            foreach (var group in Commands)
            {
                int visibleCommands = 0;
                foreach (var command in group.Commands)
                {
                    if (command.Command.DisplayName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        visibleCommands++;
                        command.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        command.Visibility = Visibility.Collapsed;
                    }
                }

                group.Visibility = visibleCommands > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public class GroupSearchResult : NotifyableObject
        {
            private Visibility visibility;

            public string DisplayName { get; set; }

            public List<CommandSearchResult> Commands { get; set; }

            public Visibility Visibility
            {
                get => visibility;
                set => SetProperty(ref visibility, value);
            }

            public GroupSearchResult(CommandGroup group)
            {
                DisplayName = group.DisplayName;
                Commands = new(group.VisibleCommands.Select(x => new CommandSearchResult(x)));
            }
        }

        public class CommandSearchResult : NotifyableObject
        {
            private Visibility visibility;

            public Command Command { get; set; }

            public Visibility Visibility
            {
                get => visibility;
                set => SetProperty(ref visibility, value);
            }

            public CommandSearchResult(Command command)
            {
                Command = command;
            }
        }
    }
}
