using System.Runtime.InteropServices;
using System.Text;

namespace PixiEditor.MacOs;

internal static class MacOsInterop
{
    private const string CoreFoundationLib = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    private const string CarbonLib = "/System/Library/Frameworks/Carbon.framework/Carbon";
    private static readonly IntPtr kTISPropertyInputSourceID = CFStringCreate("TISPropertyUnicodeKeyLayoutData");

    [DllImport(CarbonLib)]
    private static extern IntPtr TISCopyCurrentKeyboardLayoutInputSource();
    
    [DllImport(CarbonLib)]
    private static extern IntPtr TISGetInputSourceProperty(IntPtr inputSource, IntPtr propertyKey);

    [DllImport(CoreFoundationLib)]
    private static extern IntPtr CFDataGetBytePtr(IntPtr data);

    [DllImport(CarbonLib)]
    private static extern short UCKeyTranslate(
        IntPtr keyLayout,
        ushort virtualKeyCode,
        ushort keyAction,
        UInt32 modifierKeyState,
        UInt32 keyboardType,
        uint keyTranslateOptions,
        ref uint deadKeyState,
        int maxLength,
        out int actualLength,
        ushort[] unicodeString);

    public static string? GetSymbolFromKey(ushort virtualKeyCode, uint modifiers)
    {
        IntPtr layout = TISCopyCurrentKeyboardLayoutInputSource();
        if (layout == IntPtr.Zero)
            return null;

        
        IntPtr layoutData = TISGetInputSourceProperty(layout, kTISPropertyInputSourceID);
        if (layoutData == IntPtr.Zero)
            return null;

        IntPtr keyLayoutPtr = CFDataGetBytePtr(layoutData);
        if (keyLayoutPtr == IntPtr.Zero)
            return null;

        // Translate the key code into a symbol
        var state = 0u;
        var output = new ushort[255];
        UCKeyTranslate(keyLayoutPtr, virtualKeyCode, 3 /* kUCKeyActionDisplay */, modifiers, 0, 0, ref state, output.Length, out var actualLength, output);

        return actualLength > 0 ? UshortArrayToString(output, (int)actualLength) : null;
    }
    
    private static string UshortArrayToString(ushort[] array, int length)
    {
        // Convert ushort[] to byte[] for Encoding.Unicode.GetString
        byte[] bytes = new byte[length * 2];
        Buffer.BlockCopy(array, 0, bytes, 0, length * 2);
        return Encoding.Unicode.GetString(bytes, 0, length * 2);
    }
    
    [DllImport(CoreFoundationLib)]
    private static extern IntPtr CFStringCreateWithCString(IntPtr allocator, string cStr, UInt32 encoding);

    private static IntPtr CFStringCreate(string str)
    {
        return CFStringCreateWithCString(IntPtr.Zero, str, 134217984);
    }
}
