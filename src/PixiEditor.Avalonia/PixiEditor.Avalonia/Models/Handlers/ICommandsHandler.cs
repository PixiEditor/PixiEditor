using PixiEditor.Models.Commands;

namespace PixiEditor.Models.Containers;

internal interface ICommandsHandler : IHandler
{
    public CommandController CommandController { get; }
}
