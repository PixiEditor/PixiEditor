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

        try
        {
            string prefixed =
                PrefixedNameUtility.ToCommandUniqueName(Extension.Metadata.UniqueName, metadata.UniqueName, false);
            metadata.UniqueName = prefixed;
            Api.Commands.RegisterCommand(metadata, InvokeCommandInvoked);
        }
        catch (ArgumentException ex)
        {
            Api.Logger.LogError(ex.Message);
        }
    }

    [ApiFunction("command_exists")]
    internal bool CommandExists(string commandName)
    {
        CommandModule commandModule = Extension.GetModule<CommandModule>();
        return commandModule.CommandProvider.CommandExists(commandName);
    }

    [ApiFunction("invoke_command")]
    internal void InvokeCommand(string commandName)
    {
        CommandModule commandModule = Extension.GetModule<CommandModule>();

        string prefixedName = PrefixedNameUtility.ToCommandUniqueName(Extension.Metadata.UniqueName, commandName, true);

        if (!commandModule.CommandProvider.CommandExists(prefixedName))
        {
            return;
        }

        if (commandModule.CommandSupervisor.ValidateCommandPermissions(prefixedName, Extension))
        {
            Api.Commands.InvokeCommand(prefixedName);
        }
        else
        {
            Api.Logger.LogError($"Command {prefixedName} is not accessible from {Metadata.UniqueName} extension.");
        }
    }

    [ApiFunction("invoke_command_null_param")]
    internal void InvokeCommandNullParam(string commandName)
    {
        CommandModule commandModule = Extension.GetModule<CommandModule>();
        commandModule!.InvokeCommandGeneric(commandName, null);
    }

    [ApiFunction("invoke_command_string")]
    internal void InvokeCommandString(string commandName, string parameter)
    {
        CommandModule commandModule = Extension.GetModule<CommandModule>();
        commandModule!.InvokeCommandGeneric(commandName, parameter);
    }

    [ApiFunction("invoke_command_int")]
    internal void InvokeCommandInt(string commandName, int parameter)
    {
        CommandModule commandModule = Extension.GetModule<CommandModule>();
        commandModule!.InvokeCommandGeneric(commandName, parameter);
    }

    [ApiFunction("invoke_command_bool")]
    internal void InvokeCommandBool(string commandName, bool parameter)
    {
        CommandModule commandModule = Extension.GetModule<CommandModule>();
        commandModule!.InvokeCommandGeneric(commandName, parameter);
    }

    [ApiFunction("invoke_command_float")]
    internal void InvokeCommandFloat(string commandName, float parameter)
    {
        CommandModule commandModule = Extension.GetModule<CommandModule>();
        commandModule!.InvokeCommandGeneric(commandName, parameter);
    }

    [ApiFunction("invoke_command_double")]
    internal void InvokeCommandDouble(string commandName, double parameter)
    {
        CommandModule commandModule = Extension.GetModule<CommandModule>();
        commandModule!.InvokeCommandGeneric(commandName, parameter);
    }

    [ApiFunction("invoke_command_bytes")]
    internal void InvokeCommandBytes(string commandName, Span<byte> parameter)
    {
        CommandModule commandModule = Extension.GetModule<CommandModule>();

        byte[] bytes = new byte[parameter.Length];
        parameter.CopyTo(bytes);

        commandModule!.InvokeCommandGeneric(commandName, bytes);
    }
}
