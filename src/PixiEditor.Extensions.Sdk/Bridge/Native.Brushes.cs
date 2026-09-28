using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern int register_brush_data_source(string name, IntPtr brushesArray, int brushesArrayLength);
}
