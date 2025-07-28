using Avalonia.Input;

namespace PixiEditor.OperatingSystem;

public interface IInputKeys
{
    /// <summary>
    ///     Returns the character of the <paramref name="key"/> mapped to the users keyboard layout
    /// </summary>
    public string GetKeyboardKey(Key key, bool forceInvariant = false);

    public bool ModifierUsesSymbol(KeyModifiers modifier);
}
