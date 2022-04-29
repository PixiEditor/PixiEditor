using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.UserPreferences;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    [Command.Group("PixiEditor.Debug", "Debug")]
    public class DebugViewModel : SubViewModel<ViewModelMain>
    {
        public bool IsDebugBuild { get; set; }

        public bool IsDebugModeEnabaled { get; set; }

        public bool UseDebug { get; set; }

        public DebugViewModel(ViewModelMain owner, IPreferences preferences)
            : base(owner)
        {
            SetDebug();
            preferences.AddCallback<bool>("IsDebugModeEnabled", UpdateDebugMode);
            UpdateDebugMode(preferences.GetPreference<bool>("IsDebugModeEnabled"));
        }

        [Command.Basic("#DEBUG#PixiEditor.Debug.OpenTempDirectory", "%Temp%/PixiEditor", "Open Temp Directory", "Open Temp Directory")]
        [Command.Basic("#DEBUG#PixiEditor.Debug.OpenLocalAppDataDirectory", "%LocalAppData%/PixiEditor", "Open Local AppData Directory", "Open Local AppData Directory")]
        [Command.Basic("#DEBUG#PixiEditor.Debug.OpenRoamingAppDataDirectory", "%AppData%/PixiEditor", "Open Roaming AppData Directory", "Open Roaming AppData Directory")]
        public static void OpenFolder(string path)
        {
            ProcessHelpers.ShellExecuteEV(path);
        }

        [Command.Basic("#DEBUG#PixiEditor.Debug.OpenInstallDirectory", "Open Installation Directory", "Open Installation Directory")]
        public static void OpenInstallLocation()
        {
            ProcessHelpers.ShellExecuteEV(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        [Command.Basic("#DEBUG#PixiEditor.Debug.Crash", "Crash", "Crash Application")]
        public static void Crash() => throw new InvalidOperationException("User requested to crash :c");

        [Command.Basic("#DEBUG#PixiEditor.Debug.DeleteUserPreferences", @"%appdata%\PixiEditor\user_preferences.json", "Delete User Preferences (Roaming)", "Delete User Preferences (Roaming AppData)")]
        [Command.Basic("#DEBUG#PixiEditor.Debug.DeleteEditorData", @"%localappdata%\PixiEditor\editor_data.json", "Delete Editor Data (Local)", "Delete Editor Data (Local AppData)")]
        public static void DeleteFile(string path)
        {
            string file = Environment.ExpandEnvironmentVariables(path);
            if (!File.Exists(file))
            {
                NoticeDialog.Show($"File {path} does not exist\n(Full Path: {file})", "File not found");
                return;
            }

            if (ConfirmationDialog.Show($"Are you sure you want to delete {path}?\nThis data will be lost for all installations.\n(Full Path: {file})", "Are you sure?") == Models.Enums.ConfirmationType.Yes)
            {
                File.Delete(file);
            }
        }

        [Conditional("DEBUG")]
        private void SetDebug() => IsDebugBuild = true;

        private void UpdateDebugMode(bool setting) => UseDebug = IsDebugBuild || IsDebugModeEnabaled;
    }
}