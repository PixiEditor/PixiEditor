using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions;
public record struct CreateStructureMember_Action : IMakeChangeAction
{
    public CreateStructureMember_Action(Guid parentGuid, int index, StructureMemberType type)
    {
        ParentGuid = parentGuid;
        Index = index;
        Type = type;
    }

    public Guid ParentGuid { get; init; }
    public int Index { get; init; }
    public StructureMemberType Type { get; init; }

    IChange IMakeChangeAction.CreateCorrespondingChange()
    {
        return new CreateStructureMember_Change(ParentGuid, Index, Type);
    }
}

public record struct DeleteStructureMember_Action : IMakeChangeAction
{
    public DeleteStructureMember_Action(Guid guidValue)
    {
        GuidValue = guidValue;
    }

    public Guid GuidValue { get; }

    IChange IMakeChangeAction.CreateCorrespondingChange()
    {
        return new DeleteStructureMember_Change(GuidValue);
    }
}

public record struct MoveStructureMember_Action : IMakeChangeAction
{
    public MoveStructureMember_Action(Guid member, Guid targetFolder, int index)
    {
        Member = member;
        TargetFolder = targetFolder;
        Index = index;
    }

    public Guid Member { get; init; }
    public Guid TargetFolder { get; init; }
    public int Index { get; init; }

    IChange IMakeChangeAction.CreateCorrespondingChange()
    {
        return new MoveStructureMember_Change(Member, TargetFolder, Index);
    }
}

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

public record struct SetStructureMemberVisibility_Action : IMakeChangeAction
{
    public SetStructureMemberVisibility_Action(bool isVisible, Guid guidValue)
    {
        this.isVisible = isVisible;
        GuidValue = guidValue;
    }

    public bool isVisible { get; init; }
    public Guid GuidValue { get; init; }

    IChange IMakeChangeAction.CreateCorrespondingChange()
    {
        return new StructureMemberProperties_Change(GuidValue) { NewIsVisible = isVisible };
    }
}
