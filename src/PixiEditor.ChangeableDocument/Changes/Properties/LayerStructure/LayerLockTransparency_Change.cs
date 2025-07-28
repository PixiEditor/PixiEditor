using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties.LayerStructure;
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

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindNode(layerGuid, out Node layer))
            return false;

        if (layer is not ITransparencyLockable lockable)
               return false;

        originalValue = lockable.LockTransparency;
        if (originalValue == newValue)
            return false;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ((ITransparencyLockable)target.FindNodeOrThrow<Node>(layerGuid)).LockTransparency = newValue;
        ignoreInUndo = false;
        return new LayerLockTransparency_ChangeInfo(layerGuid, newValue);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        ((ITransparencyLockable)target.FindNodeOrThrow<Node>(layerGuid)).LockTransparency = originalValue;
        return new LayerLockTransparency_ChangeInfo(layerGuid, originalValue);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is LayerLockTransparency_Change change && change.layerGuid == layerGuid;
    }
}
