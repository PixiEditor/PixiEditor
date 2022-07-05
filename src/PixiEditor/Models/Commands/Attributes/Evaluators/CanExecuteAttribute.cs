namespace PixiEditor.Models.Commands.Attributes;

internal partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    internal class CanExecuteAttribute : EvaluatorAttribute
    {
        public string[] NamesOfRequiredCanExecuteEvaluators { get; }

        public CanExecuteAttribute(string name) : base(name)
        {
            NamesOfRequiredCanExecuteEvaluators = Array.Empty<string>();
        }

        public CanExecuteAttribute(string name, params string[] requires) : base(name)
        {
            NamesOfRequiredCanExecuteEvaluators = requires;
        }
    }
}
