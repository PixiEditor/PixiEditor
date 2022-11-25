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
        
        MaybeUpdateKeyState(key, args.IsRepeat, KeyStates.Down, keyboardState, OnAnyKeyDown);
        key = ConvertRightKeys(key);
        MaybeUpdateKeyState(key, args.IsRepeat, KeyStates.Down, converterdKeyboardState, OnConvertedKeyDown);
    }

    public void KeyUpInlet(KeyEventArgs args)
    {
        Key key = args.Key;
        if (key == Key.System)
            key = args.SystemKey;
        
        MaybeUpdateKeyState(key, args.IsRepeat, KeyStates.None, keyboardState, OnAnyKeyUp);
        key = ConvertRightKeys(key);
        MaybeUpdateKeyState(key, args.IsRepeat, KeyStates.None, converterdKeyboardState, OnConvertedKeyUp);
    }

    private void MaybeUpdateKeyState(
        Key key,
        bool isRepeatFromArgs,
        KeyStates newKeyState,
        Dictionary<Key, KeyStates> targetKeyboardState,
        EventHandler<FilteredKeyEventArgs>? eventToRaise)
    {
        bool keyWasUpdated = UpdateKeyState(key, newKeyState, targetKeyboardState);
        bool isRepeat = isRepeatFromArgs && !keyWasUpdated;
        if (!isRepeat && !keyWasUpdated)
            return;
        var (shift, ctrl, alt) = GetModifierStates();
        eventToRaise?.Invoke(this, new FilteredKeyEventArgs(key, key, newKeyState, isRepeat, shift, ctrl, alt));
    }
}
