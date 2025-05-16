using System.Text;
using Avalonia.Input;
using Avalonia.Win32.Input;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Windows;

public class WindowsInputKeys : IInputKeys
{
    const string Russian = "00000419";
    const string Ukrainian = "00000422";
    const string UkrainianEnhanced = "00020422";
    const string Arabic1 = "00000401";
    const string Arabic2 = "00010401";
    const string Arabic3 = "00020401";
    private const string InvariantLayoutCode = "00000409"; // Also known as the US Layout

    private static nint? invariantLayout;

    /// <summary>
    /// Returns the character of the <paramref name="key"/> mapped to the users keyboard layout
    /// </summary>
    public string GetKeyboardKey(Key key, bool forceInvariant = false) => key switch
    {
        >= Key.NumPad0 and <= Key.Divide => $"Num {GetMappedKey(key, forceInvariant)}",
        Key.Space => nameof(Key.Space),
        Key.Tab => nameof(Key.Tab),
        Key.Return => "↵",
        Key.Back => "Backspace",
        Key.Escape => "Esc",
        _ => GetMappedKey(key, forceInvariant),
    };

    public bool ModifierUsesSymbol(KeyModifiers modifier)
    {
        return false;
    }

    private static string GetMappedKey(Key key, bool forceInvariant)
    {
        int virtualKey = KeyInterop.VirtualKeyFromKey(key);
        byte[] keyboardState = new byte[256];

        nint targetLayout = GetLayoutHkl(forceInvariant);

        uint scanCode = Win32.MapVirtualKeyExW((uint)virtualKey, Win32.MapType.MAPVK_VK_TO_VSC, targetLayout);

        StringBuilder stringBuilder = new(5);
        int result = Win32.ToUnicodeEx((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0, targetLayout);

        string stringResult = result switch
        {
            0 => key.ToString(),
            -1 => stringBuilder.ToString().ToUpper(),
            _ => stringBuilder[result - 1].ToString().ToUpper()
        };

        return stringResult;
    }

    private static nint GetLayoutHkl(bool forceInvariant = false)
    {
        if (forceInvariant)
        {
            invariantLayout ??= Win32.LoadKeyboardLayoutA(InvariantLayoutCode, 1);
            return invariantLayout.Value;
        }

        var builder = new StringBuilder(8);
        bool success = Win32.GetKeyboardLayoutNameW(builder);

        // Fallback to US layout for certain layouts. Do not prepend a 0x and make sure the string is 8 chars long
        // Layouts can be found here https://learn.microsoft.com/en-us/windows-hardware/manufacture/desktop/windows-language-pack-default-values?view=windows-11
        if (!success || builder.ToString() is not (Russian or Ukrainian or UkrainianEnhanced or Arabic1 or Arabic2 or Arabic3))
        {
            return Win32.GetKeyboardLayout(0);
        }

        invariantLayout ??= Win32.LoadKeyboardLayoutA(InvariantLayoutCode, 1);
        return invariantLayout.Value;
    }
}
