using PixiEditor.Models.Commands.Evaluators;

namespace PixiEditor.Models.Commands;

internal partial class Command
{
    internal class BasicCommand : Command
    {
        public object Parameter { get; init; }

        protected override object GetParameter() => Parameter;

        public BasicCommand(Action<object> onExecute, CanExecuteEvaluator canExecute) : base(onExecute, canExecute) { }
    }
}
