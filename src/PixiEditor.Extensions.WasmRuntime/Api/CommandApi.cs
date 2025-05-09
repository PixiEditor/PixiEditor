using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.CommonApi.Utilities;
using PixiEditor.Extensions.WasmRuntime.Api.Modules;
using ProtoBuf;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class CommandApi : ApiGroupHandler
{
    [ApiFunction("register_command")]
    internal void RegisterCommand(Span<byte> commandMetadata)
    {
        CommandModule commandModule = Extension.GetModule<CommandModule>();

        using MemoryStream stream = new();
        stream.Write(commandMetadata);
        stream.Seek(0, SeekOrigin.Begin);
        CommandMetadata metadata = Serializer.Deserialize<CommandMetadata>(stream);

        string originalName = metadata.UniqueName;

        void InvokeCommandInvoked()
        {
            commandModule.InvokeCommandInvoked(originalName);
        }

        string prefixed = PrefixedNameUtility.ToCommandUniqueName(Extension.Metadata.UniqueName, metadata.UniqueName);
        metadata.UniqueName = prefixed;
        Api.Commands.RegisterCommand(metadata, InvokeCommandInvoked);
    }
}
