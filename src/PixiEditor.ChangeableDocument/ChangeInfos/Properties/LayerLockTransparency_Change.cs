using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changes;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Properties;
internal class LayerLockTransparency_Change : Change
{
    private readonly Guid layerGuid;
    private bool originalValue;
    private readonly bool newValue;

    public LayerLockTransparency_Change(Guid layerGuid, bool newValue)
    {
        this.layerGuid = layerGuid;
        this.newValue = newValue;
    }

    public override void Initialize(Document target)
    {
        originalValue = ((Layer)target.FindMemberOrThrow(layerGuid)).LockTransparency;
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        ((Layer)target.FindMemberOrThrow(layerGuid)).LockTransparency = newValue;
        ignoreInUndo = originalValue == newValue;
        return new LayerLockTransparency_ChangeInfo() { GuidValue = layerGuid };
    }

    public override IChangeInfo? Revert(Document target)
    {
        ((Layer)target.FindMemberOrThrow(layerGuid)).LockTransparency = originalValue;
        return new LayerLockTransparency_ChangeInfo() { GuidValue = layerGuid };
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is LayerLockTransparency_Change;
    }
}
