namespace PixiEditor.AvaloniaUI.Models.Commands.Templates;

internal interface IShortcutFile
{
    string Filter { get; }

    ShortcutsTemplate GetShortcutsTemplate(string path);
}
