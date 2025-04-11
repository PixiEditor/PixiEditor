using Avalonia.Input;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands.Templates.Providers.Parsers;

[Serializable]
public class KeyDefinition
{
    public string[] Commands { get; set; }
    public HumanReadableKeyCombination DefaultShortcut { get; set; }
    public string[] Parameters { get; set; }
    
    public KeyDefinition() { }
    public KeyDefinition(string[] commands, HumanReadableKeyCombination defaultShortcut, params string[] parameters)
    {
        Commands = commands;
        DefaultShortcut = defaultShortcut;
        Parameters = parameters;
    }
}

public record HumanReadableKeyCombination(string key, string[] modifiers = null)
{
    public KeyCombination ToKeyCombination()
    {
        Key parsedKey = Key.None;
        KeyModifiers parsedModifiers = KeyModifiers.None;

        if (KeyParser.TryParseSpecial(key, out parsedKey) || Enum.TryParse(key, true, out parsedKey))
        {
            parsedModifiers = ParseModifiers(modifiers);
        }
        else
        {
            throw new ArgumentException($"Invalid key: {key}");
        }
        
        return new KeyCombination(parsedKey, parsedModifiers);
    }

    private KeyModifiers ParseModifiers(string[] strings)
    {
        if(strings == null || strings.Length == 0)
        {
            return KeyModifiers.None;
        }
        
        KeyModifiers modifiers = KeyModifiers.None;

        for (int i = 0; i < strings.Length; i++)
        {
            switch (strings[i].ToLower())
            {
                case "ctrl": 
                    modifiers |= KeyModifiers.Control;
                    break;
                case "alt":
                    modifiers |= KeyModifiers.Alt;
                    break;
                case "shift":
                    modifiers |= KeyModifiers.Shift;
                    break;
                case "win":
                    modifiers |= KeyModifiers.Meta;
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
