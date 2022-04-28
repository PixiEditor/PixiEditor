using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.Changes;

namespace PixiEditor.ChangeableDocument.Actions.Properties;
public record class SetLayerLockTransparency_Action : IMakeChangeAction
{
    public SetLayerLockTransparency_Action(Guid guidValue, bool lockTranparency)
    {
        GuidValue = guidValue;
        LockTranparency = lockTranparency;
    }

    public Guid GuidValue { get; }
    public bool LockTranparency { get; }

    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new LayerLockTransparency_Change(GuidValue, LockTranparency);
    }
}
