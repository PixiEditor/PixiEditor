namespace PixiEditor.Extensions.WasmRuntime;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class ApiFunctionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
