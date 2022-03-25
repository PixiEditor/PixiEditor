namespace PixiEditor.Models.Commands.Attributes;

public static partial class Evaluator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class EvaluatorAttribute : Attribute
    {
        public string Name { get; }

        public EvaluatorAttribute(string name)
        {
            Name = name;
        }
    }
}
