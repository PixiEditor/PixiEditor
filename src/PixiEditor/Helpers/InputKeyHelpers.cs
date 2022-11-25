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

        uint scanCode = MapVirtualKeyExW((uint)virtualKey, MapType.MAPVK_VK_TO_VSC, culture.KeyboardLayoutId);
        StringBuilder stringBuilder = new(3);

        int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);

        string stringResult;

        stringResult = result switch
        {
            0 => key.ToString(),
            -1 => stringBuilder.ToString().ToUpper(),
            _ => stringBuilder[result - 1].ToString().ToUpper()
        };

        return stringResult;
    }

    private enum MapType : uint
    {
        /// <summary>
        /// The uCode parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no translation, the function returns 0.
        /// </summary>
        MAPVK_VK_TO_VSC = 0x0,

        /// <summary>
        /// The uCode parameter is a scan code and is translated into a virtual-key code that does not distinguish between left- and right-hand keys. If there is no translation, the function returns 0.
        /// </summary>
        MAPVK_VSC_TO_VK = 0x1,

        /// <summary>
        /// The uCode parameter is a virtual-key code and is translated into an unshifted character value in the low order word of the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If there is no translation, the function returns 0.
        /// </summary>
        MAPVK_VK_TO_CHAR = 0x2,

        /// <summary>
        /// The uCode parameter is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys. If there is no translation, the function returns 0.
        /// </summary>
        MAPVK_VSC_TO_VK_EX = 0x3,
    }

    [DllImport("user32.dll")]
    private static extern int ToUnicode(
        uint wVirtKey,
        uint wScanCode,
        byte[] lpKeyState,
        [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
        StringBuilder pwszBuff,
        int cchBuff,
        uint wFlags);

    [DllImport("user32.dll")]
    private static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKeyExW(uint uCode, MapType uMapType, int hkl);
}
