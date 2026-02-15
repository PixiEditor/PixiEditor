using System.Text.Json;

namespace PixiEditor.Models.Commands;

internal class ShortcutFile
{
    private readonly CommandController _commands;
    public string Path { get; }

    public ShortcutFile(string path, CommandController controller)
    {
        _commands = controller;
        Path = path;

        if (!File.Exists(path))
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
        }
    }

    public void SaveShortcuts()
    {
        List<Shortcut> shortcuts = new();

        foreach (var shortcut in _commands.Commands.GetShortcuts())
        {
            foreach (var command in shortcut.Value.Where(x => x.Shortcut != x.DefaultShortcut))
            {
                Shortcut shortcutToAdd = new Shortcut(shortcut.Key, new List<string> { command.InternalName });
                shortcuts.Add(shortcutToAdd);
            }
        }
        
        ShortcutsTemplate template = new()
        {
            Shortcuts = shortcuts.ToList(),
        };

        File.WriteAllText(Path, JsonSerializer.Serialize(template));
    }

    public ShortcutsTemplate LoadTemplate() => LoadTemplate(Path);

    public static ShortcutsTemplate LoadTemplate(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists || info.Length == 0)
            return new ShortcutsTemplate();
        
        return JsonSerializer.Deserialize<ShortcutsTemplate>(File.ReadAllText(path)) ?? new ShortcutsTemplate();
    }
}
