using PixiEditor.Models.Commands.Evaluators;

namespace PixiEditor.Models.Commands;

public partial class Command
{
    public class BasicCommand : Command
    {
        public object Parameter { get; init; }

        protected override object GetParameter() => Parameter;

        public BasicCommand(Action<object> onExecute, CanExecuteEvaluator canExecute) : base(onExecute, canExecute) { }
    }
}