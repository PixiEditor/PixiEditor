using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Native
{
    public static event Action<string> CommandInvoked;

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void register_command(IntPtr metadataPtr, int length);

    [ApiExport("command_invoked")]
    internal static void OnCommandInvoked(string uniqueName)
    {
        CommandInvoked?.Invoke(uniqueName);
    }
}
