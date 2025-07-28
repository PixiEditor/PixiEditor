using Avalonia.Input;
using PixiEditor.OperatingSystem;

namespace PixiEditor.MacOs;

public class MacOsInputKeys : IInputKeys
{
    public string GetKeyboardKey(Key key, bool forceInvariant = false)
    {
        switch (key)
        {
            case Key.LWin: return "\u2318";
            case Key.RWin: return "\u2318";
            case Key.LeftCtrl: return "\u2303";
            case Key.RightCtrl: return "\u2303";
            case Key.LeftAlt: return "\u2325";
            case Key.RightAlt: return "\u2325";
            case Key.LeftShift: return "\u21E7";
            case Key.RightShift: return "\u21E7";
            case Key.CapsLock: return "\u21EA";
            case Key.Escape: return "\u238B";
            case Key.Return: return "\u23CE";
            case Key.Back: return "\u232B";
            case Key.Tab: return "\u21E5";
        }

        if (key == Key.None) return string.Empty;

        ushort? virtualKeyCode = GetVirtualKeyCode(key);
        if (virtualKeyCode == null) return string.Empty;

        string result = MacOsInterop.GetSymbolFromKey(virtualKeyCode.Value, 0).ToUpper();
        if (result.Length == 1 && char.IsControl(result[0])) return key.ToString();
        
        return result;
    }

    public bool ModifierUsesSymbol(KeyModifiers modifier)
    {
        return true;
    }

    private ushort? GetVirtualKeyCode(Key key)
    {
        return key switch
        {
            Key.A => 0x00,
            Key.S => 0x01,
            Key.D => 0x02,
            Key.F => 0x03,
            Key.H => 0x04,
            Key.G => 0x05,
            Key.Z => 0x06,
            Key.X => 0x07,
            Key.C => 0x08,
            Key.V => 0x09,
            Key.B => 0x0B,
            Key.Q => 0x0C,
            Key.W => 0x0D,
            Key.E => 0x0E,
            Key.R => 0x0F,
            Key.Y => 0x10,
            Key.T => 0x11,
            Key.D1 => 0x12,
            Key.D2 => 0x13,
            Key.D3 => 0x14,
            Key.D4 => 0x15,
            Key.D6 => 0x16,
            Key.D5 => 0x17,
            Key.OemPlus => 0x18, // '=',
            Key.D9 => 0x19,
            Key.D7 => 0x1A,
            Key.OemMinus => 0x1B, // '-'
            Key.D8 => 0x1C,
            Key.D0 => 0x1D,
            Key.OemCloseBrackets => 0x1E, // ']'
            Key.O => 0x1F,
            Key.U => 0x20,
            Key.OemOpenBrackets => 0x21, // '['
            Key.I => 0x22,
            Key.P => 0x23,
            Key.Enter => 0x24, // Return
            Key.L => 0x25,
            Key.J => 0x26,
            Key.OemQuotes => 0x27, // "'"
            Key.K => 0x28,
            Key.OemSemicolon => 0x29, // ';'
            Key.OemBackslash => 0x2A, // '\'
            Key.OemComma => 0x2B, // ','
            Key.OemQuestion => 0x2C, // '/'
            Key.N => 0x2D,
            Key.M => 0x2E,
            Key.OemPeriod => 0x2F, // '.'
            Key.Tab => 0x30,
            Key.Space => 0x31,
            Key.OemTilde => 0x32 // '~'
            ,
            Key.Back => 0x33 // Delete
            ,
            Key.Escape => 0x35,
            Key.LWin => 0x37 // Cmd (Apple)
            ,
            Key.LeftShift => 0x38,
            Key.CapsLock => 0x39,
            Key.LeftAlt => 0x3A // Option
            ,
            Key.LeftCtrl => 0x3B // Control
            ,
            Key.RightShift => 0x3C,
            Key.RightAlt => 0x3D // Right Option
            ,
            Key.RightCtrl => 0x3E // Right Control
            ,
            Key.F17 => 0x40,
            Key.VolumeUp => 0x48,
            Key.VolumeDown => 0x49,
            Key.VolumeMute => 0x4A,
            Key.F18 => 0x4F,
            Key.F19 => 0x50,
            Key.NumPad0 => 0x52,
            Key.NumPad1 => 0x53,
            Key.NumPad2 => 0x54,
            Key.NumPad3 => 0x55,
            Key.NumPad4 => 0x56,
            Key.NumPad5 => 0x57,
            Key.NumPad6 => 0x58,
            Key.NumPad7 => 0x59,
            Key.NumPad8 => 0x5B,
            Key.NumPad9 => 0x5C,
            Key.F5 => 0x60,
            Key.F6 => 0x61,
            Key.F7 => 0x62,
            Key.F3 => 0x63,
            Key.F8 => 0x64,
            Key.F9 => 0x65,
            Key.F11 => 0x67,
            Key.F13 => 0x69,
            Key.F16 => 0x6A,
            Key.F14 => 0x6B,
            Key.F10 => 0x6D,
            Key.F12 => 0x6F,
            Key.F15 => 0x71,
            Key.Help => 0x72,
            Key.Home => 0x73,
            Key.PageUp => 0x74,
            Key.Delete => 0x75 // Below the Help key
            ,
            Key.F4 => 0x76,
            Key.End => 0x77,
            Key.F2 => 0x78,
            Key.PageDown => 0x79,
            Key.F1 => 0x7A,
            Key.Left => 0x7B // Left Arrow
            ,
            Key.Right => 0x7C // Right Arrow
            ,
            Key.Down => 0x7D // Down Arrow
            ,
            Key.Up => 0x7E // Up Arrow
            ,
            _ => null
        };
    }
}
