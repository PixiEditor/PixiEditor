using System.Windows.Input;

namespace PixiEditor.Models.Events;
#nullable enable
internal class FilteredKeyEventArgs : EventArgs
{
    public FilteredKeyEventArgs(
        Key unfilteredKey, Key key, KeyStates state, bool isRepeat, bool isShiftDown, bool isCtrlDown, bool isAltDown)
    {
        UnfilteredKey = unfilteredKey;
        Key = key;
        State = state;
        IsRepeat = isRepeat;

        ModifierKeys modifiers = ModifierKeys.None;
        if (isShiftDown)
            modifiers |= ModifierKeys.Shift;
        if (isCtrlDown)
            modifiers |= ModifierKeys.Control;
        if (isAltDown)
            modifiers |= ModifierKeys.Alt;
        Modifiers = modifiers;
    }

    public ModifierKeys Modifiers { get; }
    public Key UnfilteredKey { get; }
    public Key Key { get; }
    public KeyStates State { get; }
    public bool IsRepeat { get; }
    public bool IsShiftDown => (Modifiers & ModifierKeys.Shift) != 0;
    public bool IsCtrlDown => (Modifiers & ModifierKeys.Control) != 0;
    public bool IsAltDown => (Modifiers & ModifierKeys.Alt) != 0;
}
