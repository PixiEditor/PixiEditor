using PixiEditor.ViewModels;
using System.Windows.Input;

namespace PixiEditor.Models.Commands
{
    public partial class Command
    {
        public class ToolCommand : Command
        {
            public Type ToolType { get; init; }

            public Key TransientKey { get; init; }

            protected override object GetParameter() => ToolType;

            public ToolCommand() : base(ViewModelMain.Current.ToolsSubViewModel.SetTool, CommandController.Current.CanExecuteEvaluators["PixiEditor.HasDocument"]) { }
        }
    }
}
