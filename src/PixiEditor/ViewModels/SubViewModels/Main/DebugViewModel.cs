using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using Newtonsoft.Json;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Templates.Parsers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Debug", "Debug")]
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

    [Command.Debug("PixiEditor.Debug.OpenTempDirectory", @"%Temp%\PixiEditor", "Open Temp Directory", "Open Temp Directory", IconPath = "Folder.png")]
    [Command.Debug("PixiEditor.Debug.OpenLocalAppDataDirectory", @"%LocalAppData%\PixiEditor", "Open Local AppData Directory", "Open Local AppData Directory", IconPath = "Folder.png")]
    [Command.Debug("PixiEditor.Debug.OpenRoamingAppDataDirectory", @"%AppData%\PixiEditor", "Open Roaming AppData Directory", "Open Roaming AppData Directory", IconPath = "Folder.png")]
    [Command.Debug("PixiEditor.Debug.OpenCrashReportsDirectory", @"%LocalAppData%\PixiEditor\crash_logs", "Open Crash Reports Directory", "Open Crash Reports Directory", IconPath = "Folder.png")]
    public static void OpenFolder(string path)
    {
        string expandedPath = Environment.ExpandEnvironmentVariables(path);
        if (!Directory.Exists(expandedPath))
        {
            NoticeDialog.Show($"{expandedPath} does not exist.", "Location does not exist");
            return;
        }

        ProcessHelpers.ShellExecuteEV(path);
    }
    
    [Command.Debug("PixiEditor.Debug.DumpAllCommands", "Dump All Commands", "Dump All Commands to a text file")]
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
    
    [Command.Debug("PixiEditor.Debug.GenerateKeysTemplate", "Generate key bindings template", "Generates key bindings json template")]
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

    [Command.Debug("PixiEditor.Debug.ValidateShortcutMap", "Validate Shortcut Map", "Validates shortcut map")]
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

            NoticeDialog.Show($"Empty keys: {emptyKeys}\nUnknown Commands: {unknownCommands}", "Result");
        }
    }

    [Command.Debug("PixiEditor.Debug.OpenInstallDirectory", "Open Installation Directory", "Open Installation Directory", IconPath = "Folder.png")]
    public static void OpenInstallLocation()
    {
        ProcessHelpers.ShellExecuteEV(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
    }

    [Command.Debug("PixiEditor.Debug.Crash", "Crash", "Crash Application")]
    public static void Crash() => throw new InvalidOperationException("User requested to crash :c");

    [Command.Debug("PixiEditor.Debug.DeleteUserPreferences", @"%appdata%\PixiEditor\user_preferences.json", "Delete User Preferences (Roaming)", "Delete User Preferences (Roaming AppData)")]
    [Command.Debug("PixiEditor.Debug.DeleteShortcutFile", @"%appdata%\PixiEditor\shortcuts.json", "Delete Shortcut File (Roaming)", "Delete Shortcut File (Roaming AppData)")]
    [Command.Debug("PixiEditor.Debug.DeleteEditorData", @"%localappdata%\PixiEditor\editor_data.json", "Delete Editor Data (Local)", "Delete Editor Data (Local AppData)")]
    public static void DeleteFile(string path)
    {
        string file = Environment.ExpandEnvironmentVariables(path);
        if (!File.Exists(file))
        {
            NoticeDialog.Show($"File {path} does not exist\n(Full Path: {file})", "File not found");
            return;
        }

        OptionsDialog<string> dialog = new("Are you sure?", $"Are you sure you want to delete {path}?\nThis data will be lost for all installations.\n(Full Path: {file})")
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
