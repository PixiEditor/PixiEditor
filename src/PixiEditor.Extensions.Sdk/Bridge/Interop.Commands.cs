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
}
