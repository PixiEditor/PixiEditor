using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace PixiEditor.Helpers;

internal static class InputKeyHelpers
{
    /// <summary>
    /// Returns the charcter of the <paramref name="key"/> mapped to the users keyboard layout
    /// </summary>
    public static string GetKeyboardKey(Key key) => GetKeyboardKey(key, CultureInfo.CurrentCulture);

    public static string GetKeyboardKey(Key key, CultureInfo culture) => key switch
    {
        >= Key.NumPad0 and <= Key.Divide => $"Num {GetMappedKey(key, culture)}",
        Key.Space => nameof(Key.Space),
        Key.Tab => nameof(Key.Tab),
        Key.Return => "Enter",
        Key.Back => "Backspace",
        Key.Escape => "Esc",
        _ => GetMappedKey(key, culture),
    };

    private static string GetMappedKey(Key key, CultureInfo culture)
    {
        int virtualKey = KeyInterop.VirtualKeyFromKey(key);
        byte[] keyboardState = new byte[256];

        uint scanCode = Win32.MapVirtualKeyExW((uint)virtualKey, Win32.MapType.MAPVK_VK_TO_VSC, culture.KeyboardLayoutId);
        StringBuilder stringBuilder = new(3);

        int result = Win32.ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);

        string stringResult;

        stringResult = result switch
        {
            0 => key.ToString(),
            -1 => stringBuilder.ToString().ToUpper(),
            _ => stringBuilder[result - 1].ToString().ToUpper()
        };

        return stringResult;
    }
}
