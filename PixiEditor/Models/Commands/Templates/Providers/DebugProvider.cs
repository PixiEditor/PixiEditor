using System.IO;
using System.Windows.Input;

namespace PixiEditor.Models.Commands.Templates;

public partial class ShortcutProvider
{
    private static DebugProvider Debug { get; } = new();
    
    public class DebugProvider : ShortcutProvider, IShortcutDefaults, IShortcutFile, IShortcutInstallation
    {
        private static string InstallationPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "shortcut-provider.json");

        public override string Description => "A provider for testing providers";

        public DebugProvider() : base("Debug")
        {
        }

        public ShortcutCollection DefaultShortcuts { get; } = new()
        {
            // Add shortcuts for undo and redo
            { "PixiEditor.Undo.Undo", Key.Z, ModifierKeys.Control },
            { "PixiEditor.Undo.Redo", Key.Y, ModifierKeys.Control },
            "PixiEditor.Colors.Swap"
        };
        
        public string Filter => "json (*.json)|*.json";

        public ShortcutCollection GetShortcuts(string path) => new(ShortcutFile.LoadShortcuts(path));

        public bool InstallationPresent => File.Exists(InstallationPath);
        
        public ShortcutCollection GetInstalledShortcuts() => new(ShortcutFile.LoadShortcuts(InstallationPath));
    }
}