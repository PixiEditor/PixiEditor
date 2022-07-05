using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;
internal class LayerLockTransparency_Change : Change
{
    private readonly Guid layerGuid;
    private bool originalValue;
    private readonly bool newValue;

    [GenerateMakeChangeAction]
    public LayerLockTransparency_Change(Guid layerGuid, bool newValue)
    {
        this.layerGuid = layerGuid;
        this.newValue = newValue;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember<Layer>(layerGuid, out var layer))
            return new Error();

        originalValue = layer.LockTransparency;
        if (originalValue == newValue)
            return new Error();
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        (target.FindMemberOrThrow<Layer>(layerGuid)).LockTransparency = newValue;
        ignoreInUndo = false;
        return new LayerLockTransparency_ChangeInfo(layerGuid, newValue);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        (target.FindMemberOrThrow<Layer>(layerGuid)).LockTransparency = originalValue;
        return new LayerLockTransparency_ChangeInfo(layerGuid, originalValue);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is LayerLockTransparency_Change change && change.layerGuid == layerGuid;
    }
}
