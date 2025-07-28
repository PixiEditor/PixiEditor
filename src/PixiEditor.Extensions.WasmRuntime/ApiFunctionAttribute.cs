namespace PixiEditor.Extensions.WasmRuntime;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class ApiFunctionAttribute : Attribute
{
    public string Name { get; }

    public ApiFunctionAttribute(string name)
    {
        Name = name;
    }
}
