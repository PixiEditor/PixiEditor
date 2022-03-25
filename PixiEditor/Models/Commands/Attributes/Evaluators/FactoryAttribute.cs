namespace PixiEditor.Models.Commands.Attributes;

public partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class FactoryAttribute : EvaluatorAttribute
    {
        public object Parameter { get; set; }

        public FactoryAttribute(string name)
            : base(name)
        { }
    }
}
