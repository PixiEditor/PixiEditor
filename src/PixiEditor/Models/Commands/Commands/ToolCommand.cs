using System.Windows.Input;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Commands.Commands;

internal partial class Command
{
    internal class ToolCommand : Command
    {
        public Type ToolType { get; init; }

        public Key TransientKey { get; init; }

        protected override object GetParameter() => ToolType;

        public ToolCommand() : base(ViewModelMain.Current.ToolsSubViewModel.SetTool, CommandController.Current.CanExecuteEvaluators["PixiEditor.HasDocument"]) { }
    }
}
