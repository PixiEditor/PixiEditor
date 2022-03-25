using System.Diagnostics;

namespace PixiEditor.Models.Commands.Evaluators
{
    [DebuggerDisplay("{Name,nq}")]
    public abstract class Evaluator<T>
    {
        public string Name { get; init; }

        public Func<object, T> Evaluate { get; init; }
    }
}