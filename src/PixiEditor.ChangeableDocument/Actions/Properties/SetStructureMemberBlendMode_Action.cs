using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Properties;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Actions.Properties;
public record class SetStructureMemberBlendMode_Action : IMakeChangeAction
{
    public BlendMode BlendMode { get; }
    public Guid GuidValue { get; }

    public SetStructureMemberBlendMode_Action(BlendMode blendMode, Guid guidValue)
    {
        BlendMode = blendMode;
        GuidValue = guidValue;
    }

    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new StructureMemberBlendMode_Change(BlendMode, GuidValue);
    }
}
