using PixiEditor.Models.Commands;

namespace PixiEditor.Models.Containers;

internal interface ICommandsHandler
{
    public CommandController CommandController { get; }
}
