using System.Windows.Input;
using Newtonsoft.Json;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Commands.Templates.Parsers;

[Serializable]
public class KeyDefinition
{
    public string Command { get; set; }
    public HumanReadableKeyCombination DefaultShortcut { get; set; }
    public string[] Parameters { get; set; }
    
    public KeyDefinition() { }
    public KeyDefinition(string command, HumanReadableKeyCombination defaultShortcut, params string[] parameters)
    {
        Command = command;
        DefaultShortcut = defaultShortcut;
        Parameters = parameters;
    }
}

public record HumanReadableKeyCombination(string key, string[] modifiers = null)
{
    public KeyCombination ToKeyCombination()
    {
        Key parsedKey = Key.None;
        ModifierKeys parsedModifiers = ModifierKeys.None;
        if (Enum.TryParse(key, out parsedKey))
        {
            parsedModifiers = ParseModifiers(modifiers);
        }
        else
        {
            throw new ArgumentException($"Invalid key: {key}");
        }
        
        return new KeyCombination(parsedKey, parsedModifiers);
    }

    private ModifierKeys ParseModifiers(string[] strings)
    {
        if(strings == null || strings.Length == 0)
        {
            return ModifierKeys.None;
        }
        
        ModifierKeys modifiers = ModifierKeys.None;

        for (int i = 0; i < strings.Length; i++)
        {
            switch (strings[i].ToLower())
            {
                case "ctrl": 
                    modifiers |= ModifierKeys.Control;
                    break;
                case "alt":
                    modifiers |= ModifierKeys.Alt;
                    break;
                case "shift":
                    modifiers |= ModifierKeys.Shift;
                    break;
                case "win":
                    modifiers |= ModifierKeys.Windows;
                    break;
            }
        }
        
        return modifiers;
    }

    public static HumanReadableKeyCombination FromStringCombination(string shortcutCombination)
    {
        if (!shortcutCombination.Contains('+'))
        {
            return new HumanReadableKeyCombination(shortcutCombination, null);
        }
        
        string[] split = shortcutCombination.Split('+');
        string key = split[^1];
        string[] modifiers = split[..^1];
        return new HumanReadableKeyCombination(key, modifiers);
    }
}
