using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Properties;

namespace PixiEditor.ChangeableDocument.Actions.Properties;
public record class SetStructureMemberClipToMemberBelow_Action : IMakeChangeAction
{
    public bool IsEnabled { get; }
    public Guid GuidValue { get; }

    public SetStructureMemberClipToMemberBelow_Action(bool isEnabled, Guid guidValue)
    {
        IsEnabled = isEnabled;
        GuidValue = guidValue;
    }

    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new StructureMemberClipToMemberBelow_Change(IsEnabled, GuidValue);
    }
}
