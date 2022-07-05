namespace PixiEditor.Models.Commands.Templates;

internal interface IShortcutFile
{
    string Filter { get; }

    ShortcutCollection GetShortcuts(string path);
}
