using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Evaluators;
using PixiEditor.AvaloniaUI.Models.Commands.Templates.Providers.Parsers;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Views;
using PixiEditor.AvaloniaUI.Views.Dialogs.Debugging;
using PixiEditor.AvaloniaUI.Views.Dialogs.Debugging.Localization;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.OperatingSystem;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Debug", "DEBUG", IsVisibleMenuProperty = $"{nameof(ViewModelMain.DebugSubViewModel)}.{nameof(UseDebug)}")]
internal class DebugViewModel : SubViewModel<ViewModelMain>
{
    public static bool IsDebugBuild { get; set; }

    public bool IsDebugModeEnabled { get; set; }

    private bool useDebug;

    public bool UseDebug
    {
        get => useDebug;
        set => SetProperty(ref useDebug, value);
    }

    private LocalizationKeyShowMode localizationKeyShowMode;

    public LocalizationKeyShowMode LocalizationKeyShowMode
    {
        get => localizationKeyShowMode;
        set
        {
            if (SetProperty(ref localizationKeyShowMode, value))
            {
                LocalizedString.OverridenKeyFlowMode = value;
                Owner.LocalizationProvider.ReloadLanguage();
            }
        }
    }

    private bool forceOtherFlowDirection;
    
    public bool ForceOtherFlowDirection
    {
        get => forceOtherFlowDirection;
        set
        {
            if (SetProperty(ref forceOtherFlowDirection, value))
            {
                Language.FlipFlowDirection = value;
                Owner.LocalizationProvider.ReloadLanguage();
            }
        }
    }

    public DebugViewModel(ViewModelMain owner, IPreferences preferences)
        : base(owner)
    {
        SetDebug();
        preferences.AddCallback<bool>("IsDebugModeEnabled", UpdateDebugMode);
        UpdateDebugMode("IsDebugModeEnabled", preferences.GetPreference<bool>("IsDebugModeEnabled"));
    }

    public static void OpenFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            NoticeDialog.Show(new LocalizedString("PATH_DOES_NOT_EXIST", path), "LOCATION_DOES_NOT_EXIST");
            return;
        }

        IOperatingSystem.Current.OpenFolder(path);
    }
    

    [Command.Debug("PixiEditor.Debug.IO.OpenLocalAppDataDirectory", @"PixiEditor", "OPEN_LOCAL_APPDATA_DIR", "OPEN_LOCAL_APPDATA_DIR", Icon = "Folder.png",
        MenuItemPath = "DEBUG/OPEN_LOCAL_APPDATA_DIR", MenuItemOrder = 3)]
    [Command.Debug("PixiEditor.Debug.IO.OpenCrashReportsDirectory", @"PixiEditor\crash_logs", "OPEN_CRASH_REPORTS_DIR", "OPEN_CRASH_REPORTS_DIR", Icon = "Folder.png",
        MenuItemPath = "DEBUG/OPEN_CRASH_REPORTS_DIR", MenuItemOrder = 4)]
    public static void OpenLocalAppDataFolder(string subDirectory)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), subDirectory);
        OpenFolder(path);
    }

    [Command.Debug("PixiEditor.Debug.IO.OpenRoamingAppDataDirectory", @"PixiEditor", "OPEN_ROAMING_APPDATA_DIR", "OPEN_ROAMING_APPDATA_DIR", Icon = "Folder.png",
        MenuItemPath = "DEBUG/OPEN_ROAMING_APPDATA_DIR", MenuItemOrder = 5)]
    public static void OpenAppDataFolder(string subDirectory)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), subDirectory);
        OpenFolder(path);
    }

    [Command.Debug("PixiEditor.Debug.IO.OpenTempDirectory", @"PixiEditor", "OPEN_TEMP_DIR", "OPEN_TEMP_DIR", Icon = "Folder.png",
        MenuItemPath = "DEBUG/OPEN_TEMP_DIR", MenuItemOrder = 6)]
    public static void OpenTempFolder(string subDirectory)
    {
        var path = Path.Combine(Path.GetTempPath(), subDirectory);
        OpenFolder(path);
    }

    [Command.Debug("PixiEditor.Debug.DumpAllCommands", "DUMP_ALL_COMMANDS", "DUMP_ALL_COMMANDS_DESCRIPTIVE")]
    public async Task DumpAllCommands()
    {
        await Application.Current.ForDesktopMainWindowAsync(async desktop =>
        {
            FilePickerSaveOptions options = new FilePickerSaveOptions();
            options.DefaultExtension = "txt";
            options.FileTypeChoices = new FilePickerFileType[] { new FilePickerFileType("Text") {Patterns = new [] {"*.txt"}} };
            var pickedFile = desktop.StorageProvider.SaveFilePickerAsync(options).Result;

            if (pickedFile != null)
            {
                var commands = Owner.CommandController.Commands;

                using StreamWriter writer = new StreamWriter(pickedFile.Path.LocalPath);
                foreach (var command in commands)
                {
                    writer.WriteLine($"InternalName: {command.InternalName}");
                    writer.WriteLine($"Default Shortcut: {command.DefaultShortcut}");
                    writer.WriteLine($"IsDebug: {command.IsDebug}");
                    writer.WriteLine();
                }
            }
        });
    }
    
    [Command.Debug("PixiEditor.Debug.GenerateKeysTemplate", "GENERATE_KEY_BINDINGS_TEMPLATE", "GENERATE_KEY_BINDINGS_TEMPLATE_DESCRIPTIVE")]
    public async Task GenerateKeysTemplate()
    {
        await Application.Current.ForDesktopMainWindowAsync(async desktop =>
        {
            FilePickerSaveOptions options = new FilePickerSaveOptions();
            options.DefaultExtension = "json";
            options.FileTypeChoices = new FilePickerFileType[] { new FilePickerFileType("Json") {Patterns = new [] {"*.json"}} };
            var pickedFile = await desktop.StorageProvider.SaveFilePickerAsync(options);

            if (pickedFile != null)
            {
                var commands = Owner.CommandController.Commands;

                using StreamWriter writer = new StreamWriter(pickedFile.Path.LocalPath);
                Dictionary<string, KeyDefinition> keyDefinitions = new Dictionary<string, KeyDefinition>();
                foreach (var command in commands)
                {
                    if(command.IsDebug)
                        continue;
                    keyDefinitions.Add($"(provider).{command.InternalName}", new KeyDefinition(command.InternalName, new HumanReadableKeyCombination("None"), Array.Empty<string>()));
                }

                writer.Write(JsonConvert.SerializeObject(keyDefinitions, Formatting.Indented));
                writer.Close();
                string file = await File.ReadAllTextAsync(pickedFile.Path.LocalPath);
                foreach (var command in commands)
                {
                    if(command.IsDebug)
                        continue;
                    file = file.Replace($"(provider).{command.InternalName}", "");
                }

                await File.WriteAllTextAsync(pickedFile.Path.LocalPath, file);
                IOperatingSystem.Current.OpenFolder(Path.GetDirectoryName(pickedFile.Path.LocalPath));
            }
        });
    }

    [Command.Debug("PixiEditor.Debug.ValidateShortcutMap", "VALIDATE_SHORTCUT_MAP", "VALIDATE_SHORTCUT_MAP_DESCRIPTIVE")]
    public async Task ValidateShortcutMap()
    {
        await Application.Current.ForDesktopMainWindowAsync(async desktop =>
        {
            FilePickerOpenOptions options = new FilePickerOpenOptions
                {
                    SuggestedStartLocation =
                        await desktop.StorageProvider.TryGetFolderFromPathAsync(
                            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Data",
                                "ShortcutActionMaps")),
                    FileTypeFilter = new FilePickerFileType[] { new FilePickerFileType("Json") {Patterns = new [] {"*.json"}} }
                };
            var pickedFile = desktop.StorageProvider.OpenFilePickerAsync(options).Result.FirstOrDefault();

            if (pickedFile != null)
            {
                string file = await File.ReadAllTextAsync(pickedFile.Path.LocalPath);
                var keyDefinitions = JsonConvert.DeserializeObject<Dictionary<string, KeyDefinition>>(file);
                int emptyKeys = file.Split("\"\":").Length - 1;
                int unknownCommands = 0;

                foreach (var keyDefinition in keyDefinitions)
                {
                    if (!Owner.CommandController.Commands.ContainsKey(keyDefinition.Value.Command))
                    {
                        unknownCommands++;
                    }
                }

                NoticeDialog.Show(new LocalizedString("VALIDATION_KEYS_NOTICE_DIALOG", emptyKeys, unknownCommands), "RESULT");
            }
        });
    }

    [Command.Debug("PixiEditor.Debug.ClearRecentDocument", "CLEAR_RECENT_DOCUMENTS", "CLEAR_RECENTLY_OPENED_DOCUMENTS",
        MenuItemPath = "DEBUG/DELETE/CLEAR_RECENT_DOCUMENTS")]
    public void ClearRecentDocuments()
    {
        Owner.FileSubViewModel.RecentlyOpened.Clear();
        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.RecentlyOpened, Array.Empty<object>());
    }

    [Command.Debug("PixiEditor.Debug.OpenCommandDebugWindow", "OPEN_CMD_DEBUG_WINDOW", "OPEN_CMD_DEBUG_WINDOW",
        MenuItemPath = "DEBUG/OPEN_COMMAND_DEBUG_WINDOW", MenuItemOrder = 0)]
    public void OpenCommandDebugWindow()
    {
        new CommandDebugPopup().Show();
    }

    [Command.Debug("PixiEditor.Debug.OpenPointerDebugWindow", "Open pointer debug window", "Open pointer debug window",
        MenuItemPath = "DEBUG/Open pointer debug window", MenuItemOrder = 1)]
    public void OpenPointerDebugWindow()
    {
        new PointerDebugPopup().Show();
    }

    [Command.Debug("PixiEditor.Debug.OpenLocalizationDebugWindow", "OPEN_LOCALIZATION_DEBUG_WINDOW", "OPEN_LOCALIZATION_DEBUG_WINDOW",
        MenuItemPath = "DEBUG/OPEN_LOCALIZATION_DEBUG_WINDOW", MenuItemOrder = 2)]
    public void OpenLocalizationDebugWindow()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.Windows.OfType<LocalizationDebugWindow>().FirstOrDefault(new LocalizationDebugWindow());
            window.Show();
            window.Activate();
        }

    }

    [Command.Internal("PixiEditor.Debug.SetLanguageFromFilePicker")]
    public async Task SetLanguageFromFilePicker()
    {
        await Application.Current.ForDesktopMainWindowAsync(async desktop =>
        {
            FilePickerOpenOptions options = new FilePickerOpenOptions
            {
                SuggestedStartLocation =
                    await desktop.StorageProvider.TryGetFolderFromPathAsync(
                        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Data",
                            "Languages")),
                FileTypeFilter = new FilePickerFileType[] { new FilePickerFileType("key-value json") {Patterns = new [] {"*.json"}} }
            };
            var pickedFile = desktop.StorageProvider.OpenFilePickerAsync(options).Result.FirstOrDefault();

            if (pickedFile != null)
            {
                Owner.LocalizationProvider.LoadDebugKeys(
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        await File.ReadAllTextAsync(pickedFile.Path.LocalPath)),
                    false);
            }
        });
    }

    [Command.Debug("PixiEditor.Debug.IO.OpenInstallDirectory", "OPEN_INSTALLATION_DIR", "OPEN_INSTALLATION_DIR", Icon = "Folder.png",
        MenuItemPath = "DEBUG/OPEN_INSTALLATION_DIR", MenuItemOrder = 8)]
    public static void OpenInstallLocation()
    {
        IOperatingSystem.Current.OpenFolder(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
    }

    [Command.Debug("PixiEditor.Debug.Crash", "CRASH", "CRASH_APP",
        MenuItemPath = "DEBUG/CRASH", MenuItemOrder = 9)]
    public static void Crash() => throw new InvalidOperationException("User requested to crash :c");

    [Command.Debug("PixiEditor.Debug.DeleteUserPreferences", @"%appdata%\PixiEditor\user_preferences.json", "DELETE_USR_PREFS", "DELETE_USR_PREFS",
        MenuItemPath = "DEBUG/DELETE/USER_PREFS", MenuItemOrder = 10)]
    [Command.Debug("PixiEditor.Debug.DeleteShortcutFile", @"%appdata%\PixiEditor\shortcuts.json", "DELETE_SHORTCUT_FILE", "DELETE_SHORTCUT_FILE",
        MenuItemPath = "DEBUG/DELETE/SHORTCUT_FILE", MenuItemOrder = 11)]
    [Command.Debug("PixiEditor.Debug.DeleteEditorData", @"%localappdata%\PixiEditor\editor_data.json", "DELETE_EDITOR_DATA", "DELETE_EDITOR_DATA",
        MenuItemPath = "DEBUG/DELETE/EDITOR_DATA", MenuItemOrder = 12)]
    public static async Task DeleteFile(string path)
    {
        if (MainWindow.Current is null)
            return;
        
        string file = Environment.ExpandEnvironmentVariables(path);
        if (!File.Exists(file))
        {
            NoticeDialog.Show(new LocalizedString("File {0} does not exist\n(Full Path: {1})", path, file), "FILE_NOT_FOUND");
            return;
        }

        OptionsDialog<string> dialog = new("ARE_YOU_SURE", new LocalizedString("ARE_YOU_SURE_PATH_FULL_PATH", path, file), MainWindow.Current)
        {
            // TODO: seems like this should be localized strings
            { new LocalizedString("YES"), x => File.Delete(file) },
            new LocalizedString("CANCEL")
        };

        await dialog.ShowDialog();
    }

    [Conditional("DEBUG")]
    private static void SetDebug() => IsDebugBuild = true;

    private void UpdateDebugMode(string name, bool setting)
    {
        IsDebugModeEnabled = setting;
        UseDebug = IsDebugBuild || IsDebugModeEnabled;
    }
}
