using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Debug;
using Drawie.Interop.Avalonia.Core;
using DrawiEngine;
using Newtonsoft.Json;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Templates.Providers.Parsers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs.Debugging;
using PixiEditor.Views.Dialogs.Debugging.Localization;

namespace PixiEditor.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Debug", "DEBUG",
    IsVisibleMenuProperty = $"{nameof(ViewModelMain.DebugSubViewModel)}.{nameof(UseDebug)}")]
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

    public bool ModifiedEditorData { get; set; }

    public DebugViewModel(ViewModelMain owner)
        : base(owner)
    {
        SetDebug();
        PixiEditorSettings.Debug.IsDebugModeEnabled.ValueChanged += UpdateDebugMode;
        UpdateDebugMode(null, PixiEditorSettings.Debug.IsDebugModeEnabled.Value);
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


    [Command.Debug("PixiEditor.Debug.IO.OpenLocalAppDataDirectory", @"PixiEditor", "OPEN_LOCAL_APPDATA_DIR",
        "OPEN_LOCAL_APPDATA_DIR",
        MenuItemPath = "DEBUG/OPEN_LOCAL_APPDATA_DIR", MenuItemOrder = 5, Icon = PixiPerfectIcons.Folder,
        AnalyticsTrack = true)]
    [Command.Debug("PixiEditor.Debug.IO.OpenCrashReportsDirectory", @"PixiEditor\crash_logs", "OPEN_CRASH_REPORTS_DIR",
        "OPEN_CRASH_REPORTS_DIR",
        MenuItemPath = "DEBUG/OPEN_CRASH_REPORTS_DIR", MenuItemOrder = 6, Icon = PixiPerfectIcons.Folder,
        AnalyticsTrack = true)]
    public static void OpenLocalAppDataFolder(string subDirectory)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            subDirectory);
        OpenFolder(path);
    }

    [Command.Debug("PixiEditor.Debug.IO.OpenRoamingAppDataDirectory", @"PixiEditor", "OPEN_ROAMING_APPDATA_DIR",
        "OPEN_ROAMING_APPDATA_DIR", Icon = PixiPerfectIcons.Folder,
        MenuItemPath = "DEBUG/OPEN_ROAMING_APPDATA_DIR", MenuItemOrder = 7)]
    public static void OpenAppDataFolder(string subDirectory)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), subDirectory);
        OpenFolder(path);
    }

    [Command.Debug("PixiEditor.Debug.IO.OpenTempDirectory", @"PixiEditor", "OPEN_TEMP_DIR", "OPEN_TEMP_DIR",
        Icon = PixiPerfectIcons.Folder,
        MenuItemPath = "DEBUG/OPEN_TEMP_DIR", MenuItemOrder = 8, AnalyticsTrack = true)]
    public static void OpenTempFolder(string subDirectory)
    {
        var path = Path.Combine(Path.GetTempPath(), subDirectory);
        OpenFolder(path);
    }

    [Command.Debug("PixiEditor.Debug.DumpGPUDiagnostics", "DUMP_GPU_DIAGNOSTICS", "DUMP_GPU_DIAGNOSTICS",
        AnalyticsTrack = true)]
    public async Task DumpGpuDiagnostics()
    {
        await Application.Current.ForDesktopMainWindowAsync(async desktop =>
        {
            FilePickerSaveOptions options = new FilePickerSaveOptions();
            options.DefaultExtension = "txt";
            options.FileTypeChoices =
                new FilePickerFileType[] { new FilePickerFileType("Text") { Patterns = new[] { "*.txt" } } };
            var pickedFile = desktop.StorageProvider.SaveFilePickerAsync(options).Result;

            if (pickedFile != null)
            {
                GpuDiagnostics diagnostics = IDrawieInteropContext.Current.GetGpuDiagnostics();

                await using StreamWriter writer = new StreamWriter(pickedFile.Path.LocalPath);
                await writer.WriteAsync(diagnostics.ToString());

                IOperatingSystem.Current.OpenFolder(pickedFile.Path.LocalPath);
            }
        });
    }

    [Command.Debug("PixiEditor.Debug.DumpAllCommands", "DUMP_ALL_COMMANDS", "DUMP_ALL_COMMANDS_DESCRIPTIVE",
        AnalyticsTrack = true)]
    public async Task DumpAllCommands()
    {
        await Application.Current.ForDesktopMainWindowAsync(async desktop =>
        {
            FilePickerSaveOptions options = new FilePickerSaveOptions();
            options.DefaultExtension = "txt";
            options.FileTypeChoices =
                new FilePickerFileType[] { new FilePickerFileType("Text") { Patterns = new[] { "*.txt" } } };
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

    [Command.Debug("PixiEditor.Debug.GenerateKeysTemplate", "GENERATE_KEY_BINDINGS_TEMPLATE",
        "GENERATE_KEY_BINDINGS_TEMPLATE_DESCRIPTIVE", AnalyticsTrack = true)]
    public async Task GenerateKeysTemplate()
    {
        await Application.Current.ForDesktopMainWindowAsync(async desktop =>
        {
            FilePickerSaveOptions options = new FilePickerSaveOptions();
            options.DefaultExtension = "json";
            options.FileTypeChoices =
                new FilePickerFileType[] { new FilePickerFileType("Json") { Patterns = new[] { "*.json" } } };
            var pickedFile = await desktop.StorageProvider.SaveFilePickerAsync(options);

            if (pickedFile != null)
            {
                var commands = Owner.CommandController.Commands;

                using StreamWriter writer = new StreamWriter(pickedFile.Path.LocalPath);
                Dictionary<string, KeyDefinition> keyDefinitions = new Dictionary<string, KeyDefinition>();
                foreach (var command in commands)
                {
                    if (command.IsDebug)
                        continue;
                    keyDefinitions.Add($"(provider).{command.InternalName}",
                        new KeyDefinition([command.InternalName], new HumanReadableKeyCombination("None"),
                            Array.Empty<string>()));
                }

                writer.Write(JsonConvert.SerializeObject(keyDefinitions, Formatting.Indented));
                writer.Close();
                string file = await File.ReadAllTextAsync(pickedFile.Path.LocalPath);
                foreach (var command in commands)
                {
                    if (command.IsDebug)
                        continue;
                    file = file.Replace($"(provider).{command.InternalName}", "");
                }

                await File.WriteAllTextAsync(pickedFile.Path.LocalPath, file);
                IOperatingSystem.Current.OpenFolder(Path.GetDirectoryName(pickedFile.Path.LocalPath));
            }
        });
    }

    [Command.Debug("PixiEditor.Debug.ValidateShortcutMap", "VALIDATE_SHORTCUT_MAP", "VALIDATE_SHORTCUT_MAP_DESCRIPTIVE",
        AnalyticsTrack = true)]
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
                FileTypeFilter =
                    new FilePickerFileType[] { new FilePickerFileType("Json") { Patterns = new[] { "*.json" } } }
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
                    foreach (var command in keyDefinition.Value.Commands)
                    {
                        if (!Owner.CommandController.Commands.ContainsKey(command))
                        {
                            unknownCommands++;
                        }
                    }
                }

                NoticeDialog.Show(new LocalizedString("VALIDATION_KEYS_NOTICE_DIALOG", emptyKeys, unknownCommands),
                    "RESULT");
            }
        });
    }

    [Command.Debug("PixiEditor.Debug.ClearRecentDocument", "CLEAR_RECENT_DOCUMENTS", "CLEAR_RECENTLY_OPENED_DOCUMENTS",
        MenuItemPath = "DEBUG/DELETE/CLEAR_RECENT_DOCUMENTS", AnalyticsTrack = true)]
    public void ClearRecentDocuments()
    {
        Owner.FileSubViewModel.RecentlyOpened.Clear();
        PixiEditorSettings.File.RecentlyOpened.Value = [];
    }

    [Command.Debug("PixiEditor.Debug.OpenCommandDebugWindow", "OPEN_CMD_DEBUG_WINDOW", "OPEN_CMD_DEBUG_WINDOW",
        MenuItemPath = "DEBUG/OPEN_COMMAND_DEBUG_WINDOW", MenuItemOrder = 0, AnalyticsTrack = true)]
    public void OpenCommandDebugWindow()
    {
        new CommandDebugPopup().Show();
    }

    [Command.Debug("PixiEditor.Debug.OpenPointerDebugWindow", "Open pointer debug window", "Open pointer debug window",
        MenuItemPath = "DEBUG/Open pointer debug window", MenuItemOrder = 1, AnalyticsTrack = true)]
    public void OpenPointerDebugWindow()
    {
        new PointerDebugPopup().Show();
    }

    [Command.Debug("PixiEditor.Debug.OpenLocalizationDebugWindow", "OPEN_LOCALIZATION_DEBUG_WINDOW",
        "OPEN_LOCALIZATION_DEBUG_WINDOW",
        MenuItemPath = "DEBUG/OPEN_LOCALIZATION_DEBUG_WINDOW", MenuItemOrder = 2, AnalyticsTrack = true)]
    public void OpenLocalizationDebugWindow()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.Windows.OfType<LocalizationDebugWindow>()
                .FirstOrDefault(new LocalizationDebugWindow());
            window.Show();
            window.Activate();
        }
    }

    [Command.Debug("PixiEditor.Debug.OpenPerformanceDebugWindow", "Open Performance Debug Window",
        "Open Performance Debug Window",
        MenuItemPath = "DEBUG/Open Performance Debug Window", MenuItemOrder = 4, AnalyticsTrack = true)]
    public void OpenPerformanceDebugWindow()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.Windows.OfType<PerformanceDebugWindow>().FirstOrDefault(new PerformanceDebugWindow());
            window.Show();
            window.Activate();
        }
    }

    [Command.Internal("PixiEditor.Debug.SetLanguageFromFilePicker", AnalyticsTrack = true)]
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
                FileTypeFilter = new FilePickerFileType[]
                {
                    new FilePickerFileType("key-value json") { Patterns = new[] { "*.json" } }
                }
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

    [Command.Debug("PixiEditor.Debug.IO.OpenInstallDirectory", "OPEN_INSTALLATION_DIR", "OPEN_INSTALLATION_DIR",
        Icon = PixiPerfectIcons.Folder,
        MenuItemPath = "DEBUG/OPEN_INSTALLATION_DIR", MenuItemOrder = 9, AnalyticsTrack = true)]
    public static void OpenInstallLocation()
    {
        IOperatingSystem.Current.OpenFolder(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
    }

    [Command.Debug("PixiEditor.Debug.Crash", "CRASH", "CRASH_APP",
        MenuItemPath = "DEBUG/CRASH", MenuItemOrder = 10, AnalyticsTrack = true)]
    public static void Crash() => throw new InvalidOperationException("User requested to crash :c");

    [Command.Debug("PixiEditor.Debug.SendCatchedCrash", "Send catched crash", "Send catched crash")]
    [Conditional("DEBUG")]
    public static void SendCatchedCrash()
    {
        try
        {
            throw new InvalidOperationException("User requested to send catched exception");
        }
        catch (InvalidOperationException exception)
        {
            CrashHelper.SendExceptionInfo(exception);
        }
    }

    [Command.Debug("PixiEditor.Debug.DeleteUserPreferences", @"%appdata%/PixiEditor/user_preferences.json",
        "DELETE_USR_PREFS", "DELETE_USR_PREFS",
        MenuItemPath = "DEBUG/DELETE/USER_PREFS", MenuItemOrder = 11, AnalyticsTrack = true)]
    [Command.Debug("PixiEditor.Debug.DeleteShortcutFile", @"%appdata%/PixiEditor/shortcuts.json",
        "DELETE_SHORTCUT_FILE", "DELETE_SHORTCUT_FILE",
        MenuItemPath = "DEBUG/DELETE/SHORTCUT_FILE", MenuItemOrder = 12, AnalyticsTrack = true)]
    [Command.Debug("PixiEditor.Debug.DeleteEditorData", @"%localappdata%/PixiEditor/editor_data.json",
        "DELETE_EDITOR_DATA", "DELETE_EDITOR_DATA",
        MenuItemPath = "DEBUG/DELETE/EDITOR_DATA", MenuItemOrder = 13, AnalyticsTrack = true)]
    public static async Task DeleteFile(string path)
    {
        if (MainWindow.Current is null)
            return;
        string[] parts = path.Split('/');
        path = Path.Combine(parts); // os specific path

        string file = Environment.ExpandEnvironmentVariables(path);
        if (!File.Exists(file))
        {
            file = Paths.ParseSpecialPathOrDefault(file);
            if (!File.Exists(file))
            {
                NoticeDialog.Show(string.Format("File {0} does not exist\n(Full Path: {1})", path, file),
                    "FILE_NOT_FOUND");
                return;
            }
        }

        OptionsDialog<string> dialog =
            new("ARE_YOU_SURE", new LocalizedString("ARE_YOU_SURE_PATH_FULL_PATH", path, file), MainWindow.Current)
            {
                // TODO: seems like this should be localized strings
                { new LocalizedString("YES"), x => File.Delete(file) }, new LocalizedString("CANCEL")
            };

        await dialog.ShowDialog();
    }

    [Conditional("DEBUG")]
    private static void SetDebug() => IsDebugBuild = true;

    private void UpdateDebugMode(Setting<bool> setting, bool value)
    {
        IsDebugModeEnabled = value;
        UseDebug = IsDebugBuild || IsDebugModeEnabled;
    }

    [Command.Debug("PixiEditor.Debug.BackupUserPreferences", @"%appdata%\PixiEditor\user_preferences.json",
        "BACKUP_USR_PREFS", "BACKUP_USR_PREFS")]
    [Command.Debug("PixiEditor.Debug.BackupEditorData", @"%localappdata%\PixiEditor\editor_data.json",
        "BACKUP_EDITOR_DATA", "BACKUP_EDITOR_DATA")]
    [Command.Debug("PixiEditor.Debug.BackupShortcutFile", @"%appdata%\PixiEditor\shortcuts.json",
        "BACKUP_SHORTCUT_FILE", "BACKUP_SHORTCUT_FILE")]
    public static void BackupFile(string path)
    {
        string file = Environment.ExpandEnvironmentVariables(path);
        string backup = $"{file}.bak";

        if (!File.Exists(file))
        {
            NoticeDialog.Show(new LocalizedString("File {0} does not exist\n(Full Path: {1})", path, file),
                "FILE_NOT_FOUND");
            return;
        }

        File.Copy(file, backup, true);
    }

    [Command.Debug("PixiEditor.Debug.LoadUserPreferencesBackup", @"%appdata%\PixiEditor\user_preferences.json",
        "LOAD_USR_PREFS_BACKUP", "LOAD_USR_PREFS_BACKUP")]
    [Command.Debug("PixiEditor.Debug.LoadEditorDataBackup", @"%localappdata%\PixiEditor\editor_data.json",
        "LOAD_EDITOR_DATA_BACKUP", "LOAD_EDITOR_DATA_BACKUP")]
    [Command.Debug("PixiEditor.Debug.LoadShortcutFileBackup", @"%appdata%\PixiEditor\shortcuts.json",
        "LOAD_SHORTCUT_FILE_BACKUP", "LOAD_SHORTCUT_FILE_BACKUP")]
    public void LoadBackupFile(string path)
    {
        if (path.EndsWith("editor_data.json"))
        {
            ModifiedEditorData = true;
        }

        string file = Environment.ExpandEnvironmentVariables(path);
        string backup = $"{file}.bak";

        if (!File.Exists(backup))
        {
            NoticeDialog.Show(new LocalizedString("File {0} does not exist\n(Full Path: {1})", path, file),
                "FILE_NOT_FOUND");
            return;
        }

        if (File.Exists(file))
        {
            OptionsDialog<string> dialog =
                new("ARE_YOU_SURE", $"Are you sure you want to overwrite {path}\n(Full Path: {file})",
                    MainWindow.Current) { { "Yes", x => File.Delete(file) }, "Cancel" };

            dialog.ShowDialog();
        }

        File.Copy(backup, file, true);
    }
}
