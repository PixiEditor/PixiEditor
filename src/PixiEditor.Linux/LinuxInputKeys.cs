using Avalonia.Input;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Linux;

internal class LinuxInputKeys : IInputKeys
{
    public string GetKeyboardKey(Key key, bool forceInvariant = false)
    {
        return "";
    }

    public bool ModifierUsesSymbol(KeyModifiers modifier) => false;
}
