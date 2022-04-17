using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Properties;

namespace PixiEditor.ChangeableDocument.Actions.Properties;

public record class SetStructureMemberVisibility_Action : IMakeChangeAction
{
    public SetStructureMemberVisibility_Action(bool isVisible, Guid guidValue)
    {
        this.isVisible = isVisible;
        GuidValue = guidValue;
    }

    public bool isVisible { get; init; }
    public Guid GuidValue { get; init; }

    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new StructureMemberIsVisible_Change(GuidValue, isVisible);
    }
}
