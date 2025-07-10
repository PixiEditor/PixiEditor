namespace PixiEditor.Extensions.Sdk.Attributes;

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ControlTypeIdAttribute : System.Attribute
{
    public string TypeId { get; }

    public ControlTypeIdAttribute(string typeId)
    {
        TypeId = typeId;
    }
}
