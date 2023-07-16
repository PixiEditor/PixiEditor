using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using Avalonia.Input;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Helpers;

internal static class InputKeyHelpers
{
    /// <summary>
    /// Returns the character of the <paramref name="key"/> mapped to the users keyboard layout
    /// </summary>
    public static string GetKeyboardKey(Key key, bool forceInvariant = false) =>
        IOperatingSystem.Current.InputKeys.GetKeyboardKey(key, forceInvariant);
}
