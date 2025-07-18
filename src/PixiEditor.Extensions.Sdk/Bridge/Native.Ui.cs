using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern IntPtr find_ui_element(string name, int elementHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern IntPtr find_ui_element_in_popup(string name, int popupHandle, int elementHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void append_element_to_native_multi_child(int atIndex, int uniqueId, IntPtr body, int bodyLen);
}
