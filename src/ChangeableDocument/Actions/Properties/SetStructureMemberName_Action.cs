using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions.Properties;

public record struct SetStructureMemberName_Action : IMakeChangeAction
{
    public SetStructureMemberName_Action(string name, Guid guidValue)
    {
        Name = name;
        GuidValue = guidValue;
    }

    public string Name { get; init; }
    public Guid GuidValue { get; init; }

    IChange IMakeChangeAction.CreateCorrespondingChange()
    {
        return new StructureMemberProperties_Change(GuidValue) { NewName = Name };
    }
}
