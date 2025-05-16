using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.CommonApi.Utilities;
using PixiEditor.Extensions.Sdk.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    public static event Action<string> CommandInvoked;
    public static void RegisterCommand(CommandMetadata command)
    {
        using MemoryStream stream = new();
        Serializer.Serialize(stream, command);
        byte[] bytes = stream.ToArray();
        Native.register_command(InteropUtility.ByteArrayToIntPtr(bytes), bytes.Length);
    }

    internal static void OnCommandInvoked(string uniqueName)
    {
        CommandInvoked?.Invoke(uniqueName);
    }

    public static void InvokeCommandGeneric(string commandName, object? parameter)
    {
        if (parameter == null)
        {
            Native.invoke_command_null_param(commandName);
        }
        else if (parameter is string str)
        {
            Native.invoke_command_string(commandName, str);
        }
        else if (parameter is int i)
        {
            Native.invoke_command_int(commandName, i);
        }
        else if (parameter is bool b)
        {
            Native.invoke_command_bool(commandName, b);
        }
        else if (parameter is float f)
        {
            Native.invoke_command_float(commandName, f);
        }
        else if (parameter is double d)
        {
            Native.invoke_command_double(commandName, d);
        }
        else if (parameter is byte[] bytes)
        {
            Native.invoke_command_bytes(commandName, InteropUtility.ByteArrayToIntPtr(bytes), bytes.Length);
        }
        else
        {
            throw new ArgumentException($"Unsupported parameter type: {parameter.GetType()}");
        }
    }
}
