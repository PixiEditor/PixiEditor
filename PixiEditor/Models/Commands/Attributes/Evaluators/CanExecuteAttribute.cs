namespace PixiEditor.Models.Commands.Attributes;

public partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class CanExecuteAttribute : EvaluatorAttribute
    {
        public string[] Requires { get; }

        public CanExecuteAttribute(string name) : base(name)
        {
            Requires = Array.Empty<string>();
        }

        public CanExecuteAttribute(string name, params string[] requires) : base(name)
        {
            Requires = requires;
        }
    }
}
