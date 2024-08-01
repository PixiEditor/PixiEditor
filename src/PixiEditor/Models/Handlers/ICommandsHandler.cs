using PixiEditor.Models.Commands;

namespace PixiEditor.Models.Handlers;

internal interface ICommandsHandler : IHandler
{
    public CommandController CommandController { get; }
}
