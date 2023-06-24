namespace PixiEditor.Models.Commands.Attributes.Evaluators;

internal partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    internal class CanExecuteAttribute : EvaluatorAttribute
    {
        public string[] NamesOfRequiredCanExecuteEvaluators { get; }

        public CanExecuteAttribute([InternalName] string name) : base(name)
        {
            NamesOfRequiredCanExecuteEvaluators = Array.Empty<string>();
        }

        public CanExecuteAttribute([InternalName] string name, params string[] requires) : base(name)
        {
            NamesOfRequiredCanExecuteEvaluators = requires;
        }
    }
}
