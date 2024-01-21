#if NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class NotNullWhenAttribute : Attribute
    {
        public bool ReturnValue { get; }

        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
    }
}
#endif
