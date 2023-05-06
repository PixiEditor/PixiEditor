using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.ViewModels.SubViewModels.UserPreferences;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using PixiEditor.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Templates;
using PixiEditor.Models.UserPreferences;
using PixiEditor.Views.Dialogs;
using PixiEditor.Exceptions;

namespace PixiEditor.ViewModels;

internal class SettingsPage : NotifyableObject
{
    private LocalizedString name;

    public LocalizedString Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public SettingsPage(string nameKey)
    {
        Name = new LocalizedString(nameKey);
    }

    public void UpdateName()
    {
        Name = new LocalizedString(Name.Key);
    }
}
internal class SettingsWindowViewModel : ViewModelBase
{
    private string searchTerm;
    private int visibleGroups;
    private int currentPage;

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

    public int CurrentPage
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
    public ObservableCollection<SettingsPage> Pages { get; }

    private static List<ICustomShortcutFormat> _customShortcutFormats;

    [Command.Internal("PixiEditor.Shortcuts.Reset")]
    public static void ResetCommand()
    {
        var dialog = new OptionsDialog<string>("ARE_YOU_SURE", new LocalizedString("WARNING_RESET_SHORTCUTS_DEFAULT"))
        {
            { new LocalizedString("YES"), x => CommandController.Current.ResetShortcuts() },
            new LocalizedString("CANCEL")
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
        _customShortcutFormats ??= ShortcutProvider.GetProviders().OfType<ICustomShortcutFormat>().ToList();
        var dialog = new OpenFileDialog();
        string filterEntries = $"PixiShorts (*.pixisc)|*.pixisc|json (*.json)|*.json{BuildCustomParsersFormat(_customShortcutFormats, out string extensionsString)}";
        extensionsString = $"*.pixisc;*json;{extensionsString}";
        dialog.Filter = $"All Shortcut ({extensionsString})|{extensionsString}|{filterEntries}|All files (*.*)|*.*";
        dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (dialog.ShowDialog().GetValueOrDefault())
        {
            List<Shortcut> shortcuts = new List<Shortcut>();
            if (!TryImport(dialog, ref shortcuts))
                return;
            
            CommandController.Current.ResetShortcuts();
            CommandController.Current.Import(shortcuts, false);
            File.Copy(dialog.FileName, CommandController.ShortcutsPath, true);
            NoticeDialog.Show("SHORTCUTS_IMPORTED_SUCCESS", "SUCCESS");
        }
        // Sometimes, focus was brought back to the last edited shortcut
        Keyboard.ClearFocus();
    }

    private static bool TryImport(OpenFileDialog dialog, ref List<Shortcut> shortcuts)
    {
        if (dialog.FileName.EndsWith(".pixisc") || dialog.FileName.EndsWith(".json"))
        {
            try
            {
                shortcuts = ShortcutFile.LoadTemplate(dialog.FileName)?.Shortcuts.ToList();
            }
            catch (Exception)
            {
                NoticeDialog.Show(title: "Error", message: "Error while reading the file");
                return false;
            }

            if (shortcuts is null)
            {
                NoticeDialog.Show("SHORTCUTS_FILE_INCORRECT_FORMAT", "INVALID_FILE");
                return false;
            }
        }
        else
        {
            var provider = _customShortcutFormats.FirstOrDefault(x =>
                x.CustomShortcutExtensions.Contains(Path.GetExtension(dialog.FileName), StringComparer.OrdinalIgnoreCase));
            if (provider is null)
            {
                NoticeDialog.Show("UNSUPPORTED_FILE_FORMAT", "INVALID_FILE");
                return false;
            }

            try
            {
                shortcuts = provider.KeysParser.Parse(dialog.FileName, false)?.Shortcuts.ToList();
            }
            catch (RecoverableException e)
            {
                NoticeDialog.Show(title: "ERROR", message: e.DisplayMessage);
                return false;
            }
        }

        return true;
    }

    private static string BuildCustomParsersFormat(IList<ICustomShortcutFormat> customFormats, out string extensionsString)
    {
        if (customFormats is null || customFormats.Count == 0)
        {
            extensionsString = string.Empty;
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        StringBuilder extensionsBuilder = new StringBuilder();
        foreach (ICustomShortcutFormat format in customFormats)
        {
            foreach (var extension in format.CustomShortcutExtensions)
            {
                string extensionWithoutDot = extension.TrimStart('.');
                extensionsBuilder.Append($"*.{extensionWithoutDot};");
                builder.Append($"|{extensionWithoutDot} (*.{extensionWithoutDot})|*.{extensionWithoutDot}");
            }
        }

        extensionsString = extensionsBuilder.ToString();
        return builder.ToString();
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
        Pages = new ObservableCollection<SettingsPage>
        {
            new SettingsPage("GENERAL"),
            new SettingsPage("DISCORD"),
            new SettingsPage("KEY_BINDINGS"),
        };

        ILocalizationProvider.Current.OnLanguageChanged += OnLanguageChanged;
        Commands = new(CommandController.Current.CommandGroups.Select(x => new GroupSearchResult(x)));
        UpdateSearchResults();
        SettingsSubViewModel = new SettingsViewModel(this);
        PreferencesSettings.Current.AddCallback("IsDebugModeEnabled", _ => UpdateSearchResults());
        VisibleGroups = Commands.Count(x => x.Visibility == Visibility.Visible);
    }

    private void UpdatePages()
    {
        foreach (var page in Pages)
        {
            page.UpdateName();
        }

        RaisePropertyChanged(nameof(Pages));
    }

    private void OnLanguageChanged(Language obj)
    {
        UpdatePages();
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
                if (command.Command.DisplayName.Value.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
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

        public LocalizedString DisplayName { get; set; }

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
