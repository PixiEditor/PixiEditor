using Avalonia.Input;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.Commands.Commands;

internal partial class Command
{
    internal class ToolCommand(IToolsHandler handler) : Command(handler.SetTool, CommandController.Current.CanExecuteEvaluators["PixiEditor.HasDocument"])
    {
        public Type ToolType { get; init; }
        public IToolHandler? Handler { get; init; }
        public Key TransientKey { get; init; }
        public bool TransientImmediate { get; init; } = false;

        public override object GetParameter() => Handler != null ? Handler : ToolType;
    }
}
