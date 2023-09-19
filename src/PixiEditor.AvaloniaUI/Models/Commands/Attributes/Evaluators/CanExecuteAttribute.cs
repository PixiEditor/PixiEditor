namespace PixiEditor.AvaloniaUI.Models.Commands.Attributes.Evaluators;

internal partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    internal class CanExecuteAttribute : EvaluatorAttribute
    {
        public string[] DependentOn { get; }

        public CanExecuteAttribute([InternalName] string name) : base(name)
        {
            DependentOn = Array.Empty<string>();
        }

        public CanExecuteAttribute([InternalName] string name, params string[] dependentOn) : base(name)
        {
            DependentOn = dependentOn;
        }
    }
}
