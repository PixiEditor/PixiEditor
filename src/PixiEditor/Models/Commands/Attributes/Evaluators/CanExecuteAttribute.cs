using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Commands.Attributes.Evaluators;

internal partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    internal class CanExecuteAttribute : EvaluatorAttribute
    {
        public string[] DependentOn { get; }

        public CanExecuteAttribute([InternalName] string name) : base(name)
        {
            DependentOn = new[] { nameof(DocumentManagerViewModel.ActiveDocument) }; // ActiveDocument will be required 99% of the time, so we'll just add it by default
        }

        public CanExecuteAttribute([InternalName] string name, params string[] dependentOn) : base(name)
        {
            DependentOn = dependentOn;
        }
    }
}
