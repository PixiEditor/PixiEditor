using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using PixiEditor.Helpers;
using PixiEditor.Localization;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Templates.Parsers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.UserPreferences;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Debug", "DEBUG")]
internal class DebugViewModel : SubViewModel<ViewModelMain>
{
    public bool IsDebugBuild { get; set; }

    public bool IsDebugModeEnabled { get; set; }

    private bool useDebug;
    public bool UseDebug
    {
        get => useDebug;
        set => SetProperty(ref useDebug, value);
    }

    public DebugViewModel(ViewModelMain owner, IPreferences preferences)
        : base(owner)
    {
        SetDebug();
        preferences.AddCallback<bool>("IsDebugModeEnabled", UpdateDebugMode);
        UpdateDebugMode(preferences.GetPreference<bool>("IsDebugModeEnabled"));
    }

    [Command.Debug("PixiEditor.Debug.OpenTempDirectory", @"%Temp%\PixiEditor", "OPEN_TEMP_DIR", "OPEN_TEMP_DIR", IconPath = "Folder.png")]
    [Command.Debug("PixiEditor.Debug.OpenLocalAppDataDirectory", @"%LocalAppData%\PixiEditor", "OPEN_LOCAL_APPDATA_DIR", "OPEN_LOCAL_APPDATA_DIR", IconPath = "Folder.png")]
    [Command.Debug("PixiEditor.Debug.OpenRoamingAppDataDirectory", @"%AppData%\PixiEditor", "OPEN_ROAMING_APPDATA_DIR", "OPEN_ROAMING_APPDATA_DIR", IconPath = "Folder.png")]
    [Command.Debug("PixiEditor.Debug.OpenCrashReportsDirectory", @"%LocalAppData%\PixiEditor\crash_logs", "OPEN_CRASH_REPORTS_DIR", "OPEN_CRASH_REPORTS_DIR", IconPath = "Folder.png")]
    public static void OpenFolder(string path)
    {
        string expandedPath = Environment.ExpandEnvironmentVariables(path);
        if (!Directory.Exists(expandedPath))
        {
            NoticeDialog.Show(new LocalizedString("PATH_DOES_NOT_EXIST", expandedPath), "LOCATION_DOES_NOT_EXIST");
            return;
        }

        ProcessHelpers.ShellExecuteEV(path);
    }
    
    [Command.Debug("PixiEditor.Debug.DumpAllCommands", "DUMP_ALL_COMMANDS", "DUMP_ALL_COMMANDS_DESCRIPTIVE")]
    public void DumpAllCommands()
    {
        SaveFileDialog dialog = new SaveFileDialog();
        var dialogResult = dialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
        {
            var commands = Owner.CommandController.Commands;

            using StreamWriter writer = new StreamWriter(dialog.FileName);
            foreach (var command in commands)
            {
                writer.WriteLine($"InternalName: {command.InternalName}");
                writer.WriteLine($"Default Shortcut: {command.DefaultShortcut}");
                writer.WriteLine($"IsDebug: {command.IsDebug}");
                writer.WriteLine();
            }
        }
    }
    
    [Command.Debug("PixiEditor.Debug.GenerateKeysTemplate", "GENERATE_KEY_BINDINGS_TEMPLATE", "GENERATE_KEY_BINDINGS_TEMPLATE_DESCRIPTIVE")]
    public void GenerateKeysTemplate()
    {
        SaveFileDialog dialog = new SaveFileDialog();
        var dialogResult = dialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
        {
            var commands = Owner.CommandController.Commands;

            using StreamWriter writer = new StreamWriter(dialog.FileName);
            Dictionary<string, KeyDefinition> keyDefinitions = new Dictionary<string, KeyDefinition>();
            foreach (var command in commands)
            {
                if(command.IsDebug)
                    continue;
                keyDefinitions.Add($"(provider).{command.InternalName}", new KeyDefinition(command.InternalName, new HumanReadableKeyCombination("None"), Array.Empty<string>()));
            }

            writer.Write(JsonConvert.SerializeObject(keyDefinitions, Formatting.Indented));
            writer.Close();
            string file = File.ReadAllText(dialog.FileName);
            foreach (var command in commands)
            {
                if(command.IsDebug)
                    continue;
                file = file.Replace($"(provider).{command.InternalName}", "");
            }
            
            File.WriteAllText(dialog.FileName, file);
            ProcessHelpers.ShellExecuteEV(dialog.FileName);
        }
    }

    [Command.Debug("PixiEditor.Debug.ValidateShortcutMap", "VALIDATE_SHORTCUT_MAP", "VALIDATE_SHORTCUT_MAP_DESCRIPTIVE")]
    public void ValidateShortcutMap()
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.Filter = "Json files (*.json)|*.json";
        dialog.InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Data", "ShortcutActionMaps");
        var dialogResult = dialog.ShowDialog();
        
        if (dialogResult.HasValue && dialogResult.Value)
        {
            string file = File.ReadAllText(dialog.FileName);
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
    }

    [Command.Debug("PixiEditor.Debug.ClearRecentDocument", "CLEAR_RECENT_DOCUMENTS", "CLEAR_RECENTLY_OPENED_DOCUMENTS")]
    public void ClearRecentDocuments()
    {
        Owner.FileSubViewModel.RecentlyOpened.Clear();
        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.RecentlyOpened, Array.Empty<object>());
    }

    [Command.Debug("PixiEditor.Debug.OpenCommandDebugWindow", "OPEN_CMD_DEBUG_WINDOW", "OPEN_CMD_DEBUG_WINDOW")]
    public void OpenCommandDebugWindow()
    {
        Mouse.OverrideCursor = Cursors.Wait;
        new CommandDebugPopup().Show();
        Mouse.OverrideCursor = null;
    }

    [Command.Debug("PixiEditor.Debug.OpenInstallDirectory", "OPEN_INSTALLATION_DIR", "OPEN_INSTALLATION_DIR", IconPath = "Folder.png")]
    public static void OpenInstallLocation()
    {
        ProcessHelpers.ShellExecuteEV(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
    }

    [Command.Debug("PixiEditor.Debug.Crash", "CRASH", "CRASH_APP")]
    public static void Crash() => throw new InvalidOperationException("User requested to crash :c");

    [Command.Debug("PixiEditor.Debug.DeleteUserPreferences", @"%appdata%\PixiEditor\user_preferences.json", "DELETE_USR_PREFS", "DELETE_USR_PREFS")]
    [Command.Debug("PixiEditor.Debug.DeleteShortcutFile", @"%appdata%\PixiEditor\shortcuts.json", "DELETE_SHORTCUT_FILE", "DELETE_SHORTCUT_FILE")]
    [Command.Debug("PixiEditor.Debug.DeleteEditorData", @"%localappdata%\PixiEditor\editor_data.json", "DELETE_EDITOR_DATA", "DELETE_EDITOR_DATA")]
    public static void DeleteFile(string path)
    {
        string file = Environment.ExpandEnvironmentVariables(path);
        if (!File.Exists(file))
        {
            NoticeDialog.Show(new LocalizedString("File {0} does not exist\n(Full Path: {1})", path, file), "FILE_NOT_FOUND");
            return;
        }

        OptionsDialog<string> dialog = new("ARE_YOU_SURE", new LocalizedString("ARE_YOU_SURE_PATH_FULL_PATH", path, file))
        {
            { "Yes", x => File.Delete(file) },
            "Cancel"
        };

        dialog.ShowDialog();
    }

    [Conditional("DEBUG")]
    private void SetDebug() => IsDebugBuild = true;

    private void UpdateDebugMode(bool setting)
    {
        IsDebugModeEnabled = setting;
        UseDebug = IsDebugBuild || IsDebugModeEnabled;
    }
}
