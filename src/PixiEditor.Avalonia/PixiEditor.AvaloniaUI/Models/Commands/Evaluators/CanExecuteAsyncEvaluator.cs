using System.Threading.Tasks;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;

namespace PixiEditor.AvaloniaUI.Models.Commands.Evaluators;

internal class CanExecuteAsyncEvaluator : Evaluator<Task<bool>>
{
    public static CanExecuteAsyncEvaluator AlwaysTrue { get; } = new StaticAsyncValueEvaluator(true);

    public static CanExecuteAsyncEvaluator AlwaysFalse { get; } = new StaticAsyncValueEvaluator(false);

    private class StaticAsyncValueEvaluator : CanExecuteAsyncEvaluator
    {
        private readonly bool value;

        public StaticAsyncValueEvaluator(bool value)
        {
            this.value = value;
        }

        public override Task<bool> CallEvaluate(Command command, object parameter) => Task.FromResult(value);
    }
}
