namespace PixiEditor.Models.Commands.Templates;

public interface IShortcutFile
{
    string Filter { get; }
    
    ShortcutCollection GetShortcuts(string path);
}