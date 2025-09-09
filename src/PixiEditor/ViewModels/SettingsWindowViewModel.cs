using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Templates;
using PixiEditor.Models.Dialogs;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.UserPreferences;
using PixiEditor.Views;
using PixiEditor.Views.Shortcuts;

namespace PixiEditor.ViewModels;

internal class SettingsPage : ObservableObject
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

internal partial class SettingsWindowViewModel : ViewModelBase
{
    private string searchTerm;
    
    [ObservableProperty]
    private int visibleGroups;
    
    [ObservableProperty]
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
            string oldValue = searchTerm;
            if (SetProperty(ref searchTerm, value) &&
                !(string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(oldValue)))
            {
                UpdateSearchResults();
                VisibleGroups = Commands.Count(x => x.IsVisible);
            }
        }
    }

    public SettingsViewModel SettingsSubViewModel { get; set; }

    public List<GroupSearchResult> Commands { get; }
    public ObservableCollection<SettingsPage> Pages { get; }

    private static List<ICustomShortcutFormat>? customShortcutFormats;

    [Command.Internal("PixiEditor.Shortcuts.Reset")]
    public static async Task ResetCommand()
    {
        await new OptionsDialog<string>("ARE_YOU_SURE", new LocalizedString("WARNING_RESET_SHORTCUTS_DEFAULT"), MainWindow.Current!)
        {
            { new LocalizedString("YES"), x => CommandController.Current.ResetShortcuts() },
            new LocalizedString("CANCEL"),
        }.ShowDialog();
    }

    [Command.Internal("PixiEditor.Shortcuts.Export")]
    public static async Task ExportShortcuts()
    {
        IStorageFolder? suggestedStartLocation = null;
        try
        {
            suggestedStartLocation =
                await MainWindow.Current!.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
        }
        catch (Exception)
        {
            // If we can't get the documents folder, we will just use the default location
            // This is not a critical error, so we can ignore it
        }

        var file = await MainWindow.Current!.StorageProvider.SaveFilePickerAsync(new()
        {
            SuggestedStartLocation = suggestedStartLocation,
            FileTypeChoices = new List<FilePickerFileType>()
            {
                new FilePickerFileType("PixiShorts (*.pixisc)")
                {
                    Patterns = new List<string>
                    {
                        "*.pixisc"
                    },
                },
                new FilePickerFileType("json (*.json)")
                {
                    Patterns = new List<string>
                    {
                        "*.json"
                    },
                },
                new FilePickerFileType("All files (*.*)")
                {
                    Patterns = new List<string>
                    {
                        "*.*"
                    },
                },
            },
        });
        
        
        if (file is not null)
        {
            try
            {
                File.Copy(CommandController.ShortcutsPath, file.Path.LocalPath, true);
            }
            catch (Exception ex)
            {
                string errMessageTrimmed = ex.Message.Length > 100 ? ex.Message[..100] + "..." : ex.Message;
                NoticeDialog.Show(title: "ERROR", message: new LocalizedString("UNKNOWN_ERROR_SAVING").Value + $" {errMessageTrimmed}");
            }
        }
        
        // Sometimes, focus was brought back to the last edited shortcut
        // TODO: Keyboard.ClearFocus(); should be there but I can't find an equivalent from avalonia
    }

    [Command.Internal("PixiEditor.Shortcuts.Import")]
    public static async Task ImportShortcuts()
    {
        List<FilePickerFileType> fileTypes = new List<FilePickerFileType>
        {
            new("PixiShorts (*.pixisc)")
            {
                Patterns = new List<string>
                {
                    "*.pixisc"
                },
            },
            new("json (*.json)")
            {
                Patterns = new List<string>
                {
                    "*.json"
                },
            },
        };
        
        customShortcutFormats ??= ShortcutProvider.GetProviders().OfType<ICustomShortcutFormat>().ToList();
        AddCustomParsersFormat(customShortcutFormats, fileTypes);
        
        fileTypes.Add(new FilePickerFileType("All files (*.*)")
        {
            Patterns = new List<string>
            {
                "*.*"
            },
        });
        
        fileTypes.Insert(0, new FilePickerFileType($"All Shortcut files {string.Join(",", fileTypes.SelectMany(a => a.Patterns))}")
        {
            Patterns = fileTypes.SelectMany(a => a.Patterns).ToList(),
        });

        IStorageFolder? suggestedLocation = null;
        try
        {
            suggestedLocation =
                await MainWindow.Current!.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
        }
        catch (Exception)
        {
            // If we can't get the documents folder, we will just use the default location
            // This is not a critical error, so we can ignore it
        }

        IReadOnlyList<IStorageFile> files = await MainWindow.Current!.StorageProvider.OpenFilePickerAsync(new()
        {
            AllowMultiple = false,
            SuggestedStartLocation = suggestedLocation,
            FileTypeFilter = fileTypes,
        });
        
        if (files.Count > 0)
        {
            List<Shortcut> shortcuts = new List<Shortcut>();
            if (!TryImport(files[0], ref shortcuts))
                return;
            
            CommandController.Current.ResetShortcuts();
            CommandController.Current.Import(shortcuts, false);
            File.Copy(files[0].Path.LocalPath, CommandController.ShortcutsPath, true);
            NoticeDialog.Show("SHORTCUTS_IMPORTED_SUCCESS", "SUCCESS");
        }
        
        // Sometimes, focus was brought back to the last edited shortcut
        // TODO: Keyboard.ClearFocus(); should be there but I can't find an equivalent from avalonia
    }

    private static bool TryImport(IStorageFile file, ref List<Shortcut> shortcuts)
    {
        if (file.Name.EndsWith(".pixisc") || file.Name.EndsWith(".json"))
        {
            try
            {
                shortcuts = ShortcutFile.LoadTemplate(file.Path.LocalPath)?.Shortcuts.ToList();
            }
            catch (Exception)
            {
                NoticeDialog.Show(title: "ERROR", message: "ERROR_READING_FILE");
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
            var provider = customShortcutFormats.FirstOrDefault(x =>
                x.CustomShortcutExtensions.Contains(Path.GetExtension(file.Name), StringComparer.OrdinalIgnoreCase));
            if (provider is null)
            {
                NoticeDialog.Show("UNSUPPORTED_FILE_FORMAT", "INVALID_FILE");
                return false;
            }

            try
            {
                shortcuts = provider.KeysParser.Parse(file.Path.LocalPath, false)?.Shortcuts.ToList();
            }
            catch (RecoverableException e)
            {
                NoticeDialog.Show(title: "ERROR", message: e.DisplayMessage);
                return false;
            }
        }

        return true;
    }

    private static void AddCustomParsersFormat(IList<ICustomShortcutFormat>? customFormats, List<FilePickerFileType> listToAddTo)
    {
        if (customFormats is null || customFormats.Count == 0)
            return;

        foreach (ICustomShortcutFormat format in customFormats)
        {
            foreach (var extension in format.CustomShortcutExtensions)
            {
                string extensionWithoutDot = extension.TrimStart('.');
                listToAddTo.Add(new FilePickerFileType(extensionWithoutDot)
                {
                    Patterns = new[] { $"*.{extensionWithoutDot}" },
                });
            }
        }
    }

    [Command.Internal("PixiEditor.Shortcuts.OpenTemplatePopup")]
    public static void OpenTemplatePopup()
    {
        ImportShortcutTemplatePopup popup = new ImportShortcutTemplatePopup();
        popup.ShowDialog(MainWindow.Current!);
    }

    public SettingsWindowViewModel()
    {
        Pages = new ObservableCollection<SettingsPage>
        {
            new("GENERAL"),
            new("DISCORD"),
            new("KEY_BINDINGS"),
            new SettingsPage("UPDATES"),
            new("EXPORT"),
            new SettingsPage("SCENE"),
            new("PERFORMANCE")
        };

        ILocalizationProvider.Current.OnLanguageChanged += OnLanguageChanged;
        Commands = new(CommandController.Current.CommandGroups.Select(x => new GroupSearchResult(x)));
        UpdateSearchResults();
        SettingsSubViewModel = new SettingsViewModel(this);
        ViewModelMain.Current.Preferences.AddCallback("IsDebugModeEnabled", (_, _) => UpdateSearchResults());
        VisibleGroups = Commands.Count(x => x.IsVisible);
    }

    private void UpdatePages()
    {
        foreach (var page in Pages)
        {
            page.UpdateName();
        }

        OnPropertyChanged(nameof(Pages));
    }

    private void OnLanguageChanged(Language obj)
    {
        UpdatePages();
    }

    private void UpdateSearchResults()
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
                        command.IsVisible = true;
                    }
                    else
                    {
                        command.IsVisible = false;
                    }
                }

                group.IsVisible = visibleCommands > 0;
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
                    command.IsVisible = true;
                }
                else
                {
                    command.IsVisible = false;
                }
            }

            group.IsVisible = visibleCommands > 0;
        }
    }
}

internal partial class GroupSearchResult : ObservableObject
{
    [ObservableProperty]
    private bool isVisible;

    public LocalizedString DisplayName { get; set; }

    public List<CommandSearchResult> Commands { get; set; }

    public GroupSearchResult(CommandGroup group)
    {
        DisplayName = group.DisplayName;
        Commands = new(group.VisibleCommands.Select(x => new CommandSearchResult(x)));
    }
}

internal partial class CommandSearchResult : ObservableObject
{
    [ObservableProperty]
    private bool isVisible;

    public Models.Commands.Commands.Command Command { get; set; }

    public CommandSearchResult(Models.Commands.Commands.Command command)
    {
        Command = command;
    }
}
