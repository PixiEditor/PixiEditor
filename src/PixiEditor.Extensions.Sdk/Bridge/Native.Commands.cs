using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Native
{
    public static event Action<string> CommandInvoked;

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void register_command(IntPtr metadataPtr, int length);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void invoke_command(string commandName);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern bool command_exists(string commandName);

    [ApiExport("command_invoked")]
    internal static void OnCommandInvoked(string uniqueName)
    {
        CommandInvoked?.Invoke(uniqueName);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void invoke_command_null_param(string commandName);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void invoke_command_string(string commandName, string parameter);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void invoke_command_int(string commandName, int parameter);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void invoke_command_bool(string commandName, bool parameter);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void invoke_command_float(string commandName, float parameter);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void invoke_command_double(string commandName, double parameter);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void invoke_command_bytes(string commandName, IntPtr parameter, int length);
}
