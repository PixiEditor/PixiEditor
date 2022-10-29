using System.IO;
using System.Windows.Input;

namespace PixiEditor.Models.Commands.Templates;

internal partial class ShortcutProvider
{
    private static DebugProvider Debug { get; } = new();

    internal class DebugProvider : ShortcutProvider, IShortcutDefaults, IShortcutFile, IShortcutInstallation
    {
        private static string InstallationPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "shortcut-provider.json");

        public override string Description => "A provider for testing providers";

        public DebugProvider() : base("Debug")
        {
        }

        public List<Shortcut> DefaultShortcuts { get; } = new()
        {
            // Add shortcuts for undo and redo
            new Shortcut(Key.Z, ModifierKeys.Control, "PixiEditor.Undo.Undo"),
            new Shortcut(Key.Y, ModifierKeys.Control, "PixiEditor.Undo.Redo"),
            new Shortcut(Key.X, ModifierKeys.None, "PixiEditor.Colors.Swap")
        };

        public string Filter => "json (*.json)|*.json";

        public ShortcutsTemplate GetShortcutsTemplate(string path) => ShortcutFile.LoadTemplate(path);

        public bool InstallationPresent => File.Exists(InstallationPath);

        public ShortcutsTemplate GetInstalledShortcuts() => ShortcutFile.LoadTemplate(InstallationPath);
    }
}
