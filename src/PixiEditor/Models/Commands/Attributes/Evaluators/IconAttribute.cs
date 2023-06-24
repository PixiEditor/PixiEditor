namespace PixiEditor.Models.Commands.Attributes.Evaluators;

internal partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    internal class IconAttribute : EvaluatorAttribute
    {
        public IconAttribute([InternalName] string name)
            : base(name)
        { }
    }
}
