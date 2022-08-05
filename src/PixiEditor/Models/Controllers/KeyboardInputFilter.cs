using System.Windows.Input;
using PixiEditor.Models.Events;

namespace PixiEditor.Models.Controllers;
#nullable enable
internal class KeyboardInputFilter
{
    /// <summary> Works like a regular keydown event, but filtered </summary>
    public EventHandler<FilteredKeyEventArgs>? OnAnyKeyDown;

    /// <summary> Works like a regular keydown event, but filtered </summary>
    public EventHandler<FilteredKeyEventArgs>? OnAnyKeyUp;

    /// <summary> Ignores duplicate modifier keys </summary>
    public EventHandler<FilteredKeyEventArgs>? OnConvertedKeyDown;

    /// <summary> Ignores duplicate modifier keys </summary>
    public EventHandler<FilteredKeyEventArgs>? OnConvertedKeyUp;

    private Dictionary<Key, KeyStates> keyboardState = new();
    private Dictionary<Key, KeyStates> converterdKeyboardState = new();

    private static bool UpdateKeyState(Key key, KeyStates state, Dictionary<Key, KeyStates> keyboardState)
    {
        if (!keyboardState.ContainsKey(key))
        {
            keyboardState.Add(key, state);
            return true;
        }
        bool result = keyboardState[key] != state;
        keyboardState[key] = state;
        return result;
    }

    private Key ConvertRightKeys(Key key)
    {
        if (key == Key.RightAlt)
            return Key.LeftAlt;
        if (key == Key.RightCtrl)
            return Key.LeftCtrl;
        if (key == Key.RightShift)
            return Key.LeftShift;
        return key;
    }

    public void DeactivatedInlet(object? sender, EventArgs e)
    {
        foreach (var (key, state) in keyboardState)
        {
            if (state != KeyStates.Down)
                continue;

            UpdateKeyState(key, KeyStates.None, keyboardState);
            Key convKey = ConvertRightKeys(key);
            bool raiseConverted = UpdateKeyState(convKey, KeyStates.None, converterdKeyboardState);

            var (shift, ctrl, alt) = GetModifierStates();
            OnAnyKeyUp?.Invoke(this, new FilteredKeyEventArgs(key, key, KeyStates.None, false, shift, ctrl, alt));
            if (raiseConverted)
                OnConvertedKeyUp?.Invoke(this, new FilteredKeyEventArgs(key, key, KeyStates.None, false, shift, ctrl, alt));
        }
    }

    private (bool shift, bool ctrl, bool alt) GetModifierStates()
    {
        bool shift = converterdKeyboardState.TryGetValue(Key.LeftShift, out KeyStates shiftKey) ? shiftKey == KeyStates.Down : false;
        bool ctrl = converterdKeyboardState.TryGetValue(Key.LeftCtrl, out KeyStates ctrlKey) ? ctrlKey == KeyStates.Down : false;
        bool alt = converterdKeyboardState.TryGetValue(Key.LeftAlt, out KeyStates altKey) ? altKey == KeyStates.Down : false;
        return (shift, ctrl, alt);
    }

    public void KeyDownInlet(KeyEventArgs args)
    {
        Key key = args.Key;
        if (key == Key.System)
            key = args.SystemKey;

        if (!UpdateKeyState(key, KeyStates.Down, keyboardState))
            return;

        key = ConvertRightKeys(key);

        bool raiseConverted = UpdateKeyState(key, KeyStates.Down, converterdKeyboardState);

        var (shift, ctrl, alt) = GetModifierStates();
        OnAnyKeyDown?.Invoke(this, new FilteredKeyEventArgs(key, key, KeyStates.Down, args.IsRepeat, shift, ctrl, alt));
        if (raiseConverted)
            OnConvertedKeyDown?.Invoke(this, new FilteredKeyEventArgs(key, key, KeyStates.Down, args.IsRepeat, shift, ctrl, alt));
    }

    public void KeyUpInlet(KeyEventArgs args)
    {
        Key key = args.Key;
        if (key == Key.System)
            key = args.SystemKey;

        if (!UpdateKeyState(key, KeyStates.None, keyboardState))
            return;

        key = ConvertRightKeys(key);

        bool raiseConverted = UpdateKeyState(key, KeyStates.None, converterdKeyboardState);

        var (shift, ctrl, alt) = GetModifierStates();
        OnAnyKeyUp?.Invoke(this, new FilteredKeyEventArgs(key, key, KeyStates.None, args.IsRepeat, shift, ctrl, alt));
        if (raiseConverted)
            OnConvertedKeyUp?.Invoke(this, new FilteredKeyEventArgs(key, key, KeyStates.None, args.IsRepeat, shift, ctrl, alt));
    }
}
