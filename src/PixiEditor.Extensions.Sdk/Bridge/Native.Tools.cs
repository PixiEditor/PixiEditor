using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void register_brush_tool(IntPtr pixiFileBytes, int pixiBytesLength, IntPtr toolConfigBytes, int configBytesLength);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void add_tool_to_toolset(string toolName, string toolsetName);
}
