using Avalonia.Input;

namespace PixiEditor.Models.Commands.Attributes.Commands;

[AttributeUsage(AttributeTargets.Method)]
internal class CustomOsShortcutAttribute : Attribute
{
    public string TargetCommand { get; }
    public string ValidOs { get; }
    public Key Key { get; }
    public KeyModifiers Modifiers { get; }
    
    public CustomOsShortcutAttribute(string targetCommand, string validOs, Key key, KeyModifiers modifiers)
    {
        TargetCommand = targetCommand;
        ValidOs = validOs;
        Key = key;
        Modifiers = modifiers;
    }
}
