using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Exceptions;
using CommandAttribute = PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands.Command;

namespace PixiEditor.AvaloniaUI.Helpers;

/// <summary>
/// Helps with debugging when using XAML
/// </summary>
internal static class DesignCommandHelpers
{
    private static IEnumerable<CommandAttribute.CommandAttribute> _commands;

    public static CommandAttribute.CommandAttribute GetCommandAttribute(string name)
    {
        if (_commands == null)
        {
            _commands = Assembly
                .GetAssembly(typeof(CommandController))
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .SelectMany(x => x.GetCustomAttributes<CommandAttribute.CommandAttribute>());
        }

        var command = _commands.SingleOrDefault(x => x.InternalName == name || x.InternalName == $"#DEBUG#{name}");

        if (command == null)
        {
            throw new CommandNotFoundException(name);
        }

        return command;
    }
}
