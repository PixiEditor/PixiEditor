using System.IO;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.ViewModels.SubViewModels.UserPreferences;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.UserPreferences;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels;

internal class SettingsWindowViewModel : ViewModelBase
{
    private string searchTerm;
    private int visibleGroups;
    private string currentPage;

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

    public string CurrentPage
    {
        get => currentPage;
        set => SetProperty(ref currentPage, value);
    }

    public int VisibleGroups
    {
        get => visibleGroups;
        private set => SetProperty(ref visibleGroups, value);
    }

    public SettingsViewModel SettingsSubViewModel { get; set; }

    public List<GroupSearchResult> Commands { get; }

    [Command.Internal("PixiEditor.Shortcuts.Reset")]
    public static void ResetCommand()
    {
        var dialog = new OptionsDialog<string>("Are you sure?", "Are you sure you want to reset all shortcuts to their default value?")
        {
            { "Yes", x => CommandController.Current.ResetShortcuts() },
            "Cancel"
        }.ShowDialog();
    }

    [Command.Internal("PixiEditor.Shortcuts.Export")]
    public static void ExportShortcuts()
    {
        var dialog = new SaveFileDialog();
        dialog.Filter = "PixiShorts (*.pixisc)|*.pixisc|json (*.json)|*.json|All files (*.*)|*.*";
        dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (dialog.ShowDialog().GetValueOrDefault())
        {
            File.Copy(CommandController.ShortcutsPath, dialog.FileName, true);
        }
        // Sometimes, focus was brought back to the last edited shortcut
        Keyboard.ClearFocus();
    }

    [Command.Internal("PixiEditor.Shortcuts.Import")]
    public static void ImportShortcuts()
    {
        var dialog = new OpenFileDialog();
        dialog.Filter = "PixiShorts (*.pixisc)|*.pixisc|json (*.json)|*.json|All files (*.*)|*.*";
        dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (dialog.ShowDialog().GetValueOrDefault())
        {
            List<Shortcut> shortcuts = new List<Shortcut>();
            try
            {
                shortcuts = ShortcutFile.LoadTemplate(dialog.FileName)?.Shortcuts.ToList();
                if (shortcuts is null)
                {
                    NoticeDialog.Show("Shortcuts file was not in a valid format", "Invalid file");
                    return;
                }
            }
            catch (Exception e)
            {
                NoticeDialog.Show("Shortcuts file was not in a valid format", "Invalid file");
                return;
            }
            
            CommandController.Current.ResetShortcuts();
            CommandController.Current.Import(shortcuts, false);
            File.Copy(dialog.FileName, CommandController.ShortcutsPath, true);
            NoticeDialog.Show("Shortcuts were imported successfully", "Success");
        }
        // Sometimes, focus was brought back to the last edited shortcut
        Keyboard.ClearFocus();
    }

    [Command.Internal("PixiEditor.Shortcuts.OpenTemplatePopup")]
    public static void OpenTemplatePopup()
    {
        ImportShortcutTemplatePopup popup = new ImportShortcutTemplatePopup();
        var settingsWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
        if(settingsWindow is not null)
        {
            popup.Owner = settingsWindow;
        }
        
        popup.ShowDialog();
    }

    public SettingsWindowViewModel()
    {
        Commands = new(CommandController.Current.CommandGroups.Select(x => new GroupSearchResult(x)));
        UpdateSearchResults();
        SettingsSubViewModel = new SettingsViewModel(this);
        PreferencesSettings.Current.AddCallback("IsDebugModeEnabled", _ => UpdateSearchResults());
        VisibleGroups = Commands.Count(x => x.Visibility == Visibility.Visible);
    }

    public void UpdateSearchResults()
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            foreach (var group in Commands)
            {
                var visibleCommands = 0;

                foreach (var command in group.Commands)
                {
                    if ((command.Command.IsDebug && ViewModelMain.Current.DebugSubViewModel.UseDebug) ||
                        !command.Command.IsDebug)
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
            return;
        }

        foreach (var group in Commands)
        {
            var visibleCommands = 0;
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

    internal class GroupSearchResult : NotifyableObject
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

    internal class CommandSearchResult : NotifyableObject
    {
        private Visibility visibility;

        public Models.Commands.Commands.Command Command { get; set; }

        public Visibility Visibility
        {
            get => visibility;
            set => SetProperty(ref visibility, value);
        }

        public CommandSearchResult(Models.Commands.Commands.Command command)
        {
            Command = command;
        }
    }
}
