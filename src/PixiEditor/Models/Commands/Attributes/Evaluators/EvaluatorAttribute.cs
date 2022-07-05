namespace PixiEditor.Models.Commands.Attributes;

internal static partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    internal abstract class EvaluatorAttribute : Attribute
    {
        public string Name { get; }

        public EvaluatorAttribute(string name)
        {
            Name = name;
        }
    }
}
