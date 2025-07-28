using PixiEditor.ChangeableDocument.ChangeInfos.Root.ReferenceLayerChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;

namespace PixiEditor.ChangeableDocument.Changes.Root.ReferenceLayerChanges;

internal class DeleteReferenceLayer_Change : Change
{
    private ReferenceLayer? lastReferenceLayer;

    [GenerateMakeChangeAction]
    public DeleteReferenceLayer_Change() { }
    
    public override bool InitializeAndValidate(Document target)
    {
        if (target.ReferenceLayer is null)
            return false;
        lastReferenceLayer = target.ReferenceLayer.Clone();
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.ReferenceLayer = null;
        ignoreInUndo = false;
        return new DeleteReferenceLayer_ChangeInfo();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.ReferenceLayer = lastReferenceLayer!.Clone();
        return new SetReferenceLayer_ChangeInfo(
            target.ReferenceLayer.ImageBgra8888Bytes,
            target.ReferenceLayer.ImageSize,
            target.ReferenceLayer.Shape);
    }
}
