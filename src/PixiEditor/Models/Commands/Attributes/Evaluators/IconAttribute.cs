namespace PixiEditor.Models.Commands.Attributes;

internal partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    internal class IconAttribute : EvaluatorAttribute
    {
        public IconAttribute(string name)
            : base(name)
        { }
    }
}
