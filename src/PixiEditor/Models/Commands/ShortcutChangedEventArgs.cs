using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands;

internal class ShortcutChangedEventArgs : EventArgs
{
    public KeyCombination OldShortcut { get; }

    public KeyCombination NewShortcut { get; }

    public ShortcutChangedEventArgs(KeyCombination oldShortcut, KeyCombination newShortcut)
    {
        OldShortcut = oldShortcut;
        NewShortcut = newShortcut;
    }
}
