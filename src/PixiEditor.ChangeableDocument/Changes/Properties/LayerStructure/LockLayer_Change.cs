using PixiEditor.ChangeableDocument.ChangeInfos.Structure;

namespace PixiEditor.ChangeableDocument.Changes.Properties.LayerStructure;

internal class LockLayer_Change : Change
{
    public Guid layerGuid;
    public bool originalValue;
    public bool lockValue;

    [GenerateMakeChangeAction]
    public LockLayer_Change(Guid layerGuid, bool lockIt)
    {
        this.layerGuid = layerGuid;
        this.lockValue = lockIt;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.TryFindMember(layerGuid, out var member))
        {
            originalValue = member.IsLocked;
            return originalValue != lockValue;
        }

        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var layer = target.FindMemberOrThrow(layerGuid);
        layer.IsLocked = lockValue;
        ignoreInUndo = false;

        return new LayerLock_ChangeInfo(layerGuid, lockValue);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var layer = target.FindMemberOrThrow(layerGuid);
        layer.IsLocked = originalValue;
        return new LayerLock_ChangeInfo(layerGuid, originalValue);
    }
}
