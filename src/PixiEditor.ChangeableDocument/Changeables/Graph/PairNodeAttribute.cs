namespace PixiEditor.ChangeableDocument.Changeables.Graph;

[AttributeUsage(validOn: AttributeTargets.Class)]
public class PairNodeAttribute(Type otherType, string zoneUniqueName, bool isStartingType = false) : Attribute
{
    public Type OtherType { get; set; } = otherType;
    public bool IsStartingType { get; set; } = isStartingType;
    public string ZoneUniqueName { get; set; } = zoneUniqueName;
}
