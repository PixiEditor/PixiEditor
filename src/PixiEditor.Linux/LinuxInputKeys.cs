using Avalonia.Input;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Linux;

internal class LinuxInputKeys : IInputKeys
{
    public string GetKeyboardKey(Key key, bool forceInvariant = false)
    {
        return MapKey(key);
    }

    public bool ModifierUsesSymbol(KeyModifiers modifier) => false;

    private string MapKey(Key key)
    {
        // at the moment only latin keys are supported

        return key switch
        {
            Key.Back => "Backspace",
            Key.Tab => "Tab",
            Key.Return => "↵",
            Key.CapsLock => "Caps Lock",
            Key.Escape => "Esc",
            Key.Space => "Space",
            Key.PageUp => "Page Up",
            Key.PageDown => "Page Down",
            Key.D0 => "0",
            Key.D1 => "1",
            Key.D2 => "2",
            Key.D3 => "3",
            Key.D4 => "4",
            Key.D5 => "5",
            Key.D6 => "6",
            Key.D7 => "7",
            Key.D8 => "8",
            Key.D9 => "9",
            Key.LWin => "Super",
            Key.RWin => "Super",
            Key.NumPad0 => "0",
            Key.NumPad1 => "1",
            Key.NumPad2 => "2",
            Key.NumPad3 => "3",
            Key.NumPad4 => "4",
            Key.NumPad5 => "5",
            Key.NumPad6 => "6",
            Key.NumPad7 => "7",
            Key.NumPad8 => "8",
            Key.NumPad9 => "9",
            Key.Multiply => "*",
            Key.Add => "+",
            Key.Separator => ",",
            Key.Subtract => "-",
            Key.Decimal => ".",
            Key.Divide => "/",
            Key.NumLock => "Num Lock",
            Key.LeftShift => "Shift",
            Key.RightShift => "Shift",
            Key.LeftCtrl => "Ctrl",
            Key.RightCtrl => "Ctrl",
            Key.LeftAlt => "Alt",
            Key.RightAlt => "Alt",
            Key.OemSemicolon => ";",
            Key.OemPlus => "=",
            Key.OemComma => ",",
            Key.OemMinus => "-",
            Key.OemPeriod => ".",
            Key.OemQuestion => "/",
            Key.OemTilde => "`",
            Key.OemOpenBrackets => "[",
            Key.OemPipe => "\\",
            Key.OemCloseBrackets => "]",
            Key.OemQuotes => "'",
            Key.OemBackslash => "\\",
            Key.FnLeftArrow => "Left Arrow",
            Key.FnRightArrow => "Right Arrow",
            Key.FnUpArrow => "Up Arrow",
            Key.FnDownArrow => "Down Arrow",
            Key.MediaHome => "Home",
            _ => key.ToString()
        };
    }
}
