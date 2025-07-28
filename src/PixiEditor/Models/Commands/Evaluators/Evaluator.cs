using System.Diagnostics;
using PixiEditor.Models.Commands.Commands;

namespace PixiEditor.Models.Commands.Evaluators;

[DebuggerDisplay("{Name,nq}")]
internal abstract class Evaluator<T>
{
    public string Name { get; init; }

    public Func<object, T?> Evaluate { private get; init; }

    /// <param name="command">The command this evaluator corresponds to</param>
    /// <param name="parameter">The parameter to pass to the Evaluate function</param>
    /// <returns>The value returned by the Evaluate function</returns>
    public virtual T? CallEvaluate(Command command, object parameter) => Evaluate(parameter);
}
