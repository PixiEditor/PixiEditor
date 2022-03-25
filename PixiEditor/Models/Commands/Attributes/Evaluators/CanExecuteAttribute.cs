namespace PixiEditor.Models.Commands.Attributes;

public partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class CanExecuteAttribute : EvaluatorAttribute
    {
        public CanExecuteAttribute(string name)
            : base(name)
        { }
    }
}
