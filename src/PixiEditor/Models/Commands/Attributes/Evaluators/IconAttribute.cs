namespace PixiEditor.Models.Commands.Attributes;

public partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class IconAttribute : EvaluatorAttribute
    {
        public IconAttribute(string name)
            : base(name)
        { }
    }
}
