namespace ChangeableDocument.Actions;
public record CreateStructureMemberAction : IAction
{
    public CreateStructureMemberAction(Guid parentGuid, int index, StructureMemberType type)
    {
        ParentGuid = parentGuid;
        Index = index;
        Type = type;
    }

    public Guid ParentGuid { get; init; }
    public int Index { get; init; }
    public StructureMemberType Type { get; init; }
}

public record DeleteStructureMemberAction : IAction
{
    public DeleteStructureMemberAction(Guid guidValue)
    {
        GuidValue = guidValue;
    }

    public Guid GuidValue { get; }
}

public record MoveStructureMemberAction : IAction
{
    public MoveStructureMemberAction(Guid member, Guid targetFolder, int index)
    {
        Member = member;
        TargetFolder = targetFolder;
        Index = index;
    }

    public Guid Member { get; init; }
    public Guid TargetFolder { get; init; }
    public int Index { get; init; }
}

public record SetStructureMemberNameAction : IAction
{
    public SetStructureMemberNameAction(string name, Guid guidValue)
    {
        Name = name;
        GuidValue = guidValue;
    }

    public string Name { get; init; }
    public Guid GuidValue { get; init; }
}

public record SetStructureMemberVisibilityAction : IAction
{
    public SetStructureMemberVisibilityAction(bool isVisible, Guid guidValue)
    {
        this.isVisible = isVisible;
        GuidValue = guidValue;
    }

    public bool isVisible { get; init; }
    public Guid GuidValue { get; init; }
}
