using System.Collections.Generic;
using Avalonia.Input;
using PixiEditor.Models.Events;

namespace PixiEditor.Models.Controllers.InputDevice;
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

            var (shift, ctrl, alt, meta) = GetModifierStates();
            OnAnyKeyUp?.Invoke(this, new FilteredKeyEventArgs(key, key, KeyStates.None, false, shift, ctrl, alt, meta));
            if (raiseConverted)
                OnConvertedKeyUp?.Invoke(this, new FilteredKeyEventArgs(key, key, KeyStates.None, false, shift, ctrl, alt, meta));
        }
    }

    private (bool shift, bool ctrl, bool alt, bool meta) GetModifierStates()
    {
        bool shift = converterdKeyboardState.TryGetValue(Key.LeftShift, out KeyStates shiftKey) && shiftKey == KeyStates.Down;
        bool ctrl = converterdKeyboardState.TryGetValue(Key.LeftCtrl, out KeyStates ctrlKey) && ctrlKey == KeyStates.Down;
        bool alt = converterdKeyboardState.TryGetValue(Key.LeftAlt, out KeyStates altKey) && altKey == KeyStates.Down;
        bool meta = converterdKeyboardState.TryGetValue(Key.LWin, out KeyStates metaKey) && metaKey == KeyStates.Down;
        return (shift, ctrl, alt, meta);
    }

    public void KeyDownInlet(KeyEventArgs args)
    {
        Key key = args.Key;
        /*if (key == Key.System) TODO: Validate if this is not needed
            key = args.SystemKey;*/

        bool isRepeat = keyboardState.TryGetValue(key, out KeyStates state) ? state == KeyStates.Down : false;
        UpdateModifiers(args.KeyModifiers);

        MaybeUpdateKeyState(key, isRepeat, KeyStates.Down, keyboardState, OnAnyKeyDown);
        key = ConvertRightKeys(key);
        MaybeUpdateKeyState(key, isRepeat, KeyStates.Down, converterdKeyboardState, OnConvertedKeyDown);
    }

    private void UpdateModifiers(KeyModifiers argsKeyModifiers)
    {
        KeyStates newState = (argsKeyModifiers & KeyModifiers.Shift) != 0 ? KeyStates.Down : KeyStates.None;
        MaybeUpdateKeyState(Key.LeftShift, false, newState, converterdKeyboardState, OnConvertedKeyDown);

        newState = (argsKeyModifiers & KeyModifiers.Control) != 0 ? KeyStates.Down : KeyStates.None;
        MaybeUpdateKeyState(Key.LeftCtrl, false, newState, converterdKeyboardState, OnConvertedKeyDown);

        newState = (argsKeyModifiers & KeyModifiers.Alt) != 0 ? KeyStates.Down : KeyStates.None;
        MaybeUpdateKeyState(Key.LeftAlt, false, newState, converterdKeyboardState, OnConvertedKeyDown);

        newState = (argsKeyModifiers & KeyModifiers.Meta) != 0 ? KeyStates.Down : KeyStates.None;
        MaybeUpdateKeyState(Key.LWin, false, newState, converterdKeyboardState, OnConvertedKeyDown);
    }

    public void KeyUpInlet(KeyEventArgs args)
    {
        Key key = args.Key;
        /*if (key == Key.System) TODO: Validate if this is not needed
            key = args.SystemKey;*/

        bool isRepeat = keyboardState.TryGetValue(key, out KeyStates state) ? state == KeyStates.Down : false;

        MaybeUpdateKeyState(key, isRepeat, KeyStates.None, keyboardState, OnAnyKeyUp);
        key = ConvertRightKeys(key);
        MaybeUpdateKeyState(key, isRepeat, KeyStates.None, converterdKeyboardState, OnConvertedKeyUp);
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
        var (shift, ctrl, alt, meta) = GetModifierStates();
        eventToRaise?.Invoke(this, new FilteredKeyEventArgs(key, key, newKeyState, isRepeat, shift, ctrl, alt, meta));
    }
}
