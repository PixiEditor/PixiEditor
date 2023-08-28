using PixiEditor.AvaloniaUI.Models.Commands;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface ICommandsHandler : IHandler
{
    public CommandController CommandController { get; }
}
