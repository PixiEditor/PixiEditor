using Avalonia.Input;

namespace PixiEditor.Models.Events;
#nullable enable
internal class FilteredKeyEventArgs : EventArgs
{
    public FilteredKeyEventArgs(
        Key unfilteredKey, Key key, KeyStates state, bool isRepeat, bool isShiftDown, bool isCtrlDown, bool isAltDown, bool isMetaDown)
    {
        UnfilteredKey = unfilteredKey;
        Key = key;
        State = state;
        IsRepeat = isRepeat;

        KeyModifiers modifiers = KeyModifiers.None;
        if (isShiftDown)
            modifiers |= KeyModifiers.Shift;
        if (isCtrlDown)
            modifiers |= KeyModifiers.Control;
        if (isAltDown)
            modifiers |= KeyModifiers.Alt;
        if (isMetaDown)
            modifiers |= KeyModifiers.Meta;
        Modifiers = modifiers;
    }

    public KeyModifiers Modifiers { get; }
    public Key UnfilteredKey { get; }
    public Key Key { get; }
    public KeyStates State { get; }
    public bool IsRepeat { get; }
    public bool IsShiftDown => (Modifiers & KeyModifiers.Shift) != 0;
    public bool IsCtrlDown => (Modifiers & KeyModifiers.Control) != 0;
    public bool IsAltDown => (Modifiers & KeyModifiers.Alt) != 0;
    public bool IsMetaDown => (Modifiers & KeyModifiers.Meta) != 0;
}
