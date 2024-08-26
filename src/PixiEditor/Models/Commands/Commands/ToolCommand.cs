using Avalonia.Input;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.Commands.Commands;

internal partial class Command
{
    internal class ToolCommand(IToolsHandler handler) : Command(handler.SetTool, CommandController.Current.CanExecuteEvaluators["PixiEditor.HasDocument"])
    {
        public Type ToolType { get; init; }

        public Key TransientKey { get; init; }

        public override object GetParameter() => ToolType;
    }
}
