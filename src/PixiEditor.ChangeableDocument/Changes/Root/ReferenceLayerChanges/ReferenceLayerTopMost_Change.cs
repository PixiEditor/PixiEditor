using PixiEditor.ChangeableDocument.ChangeInfos.Root.ReferenceLayerChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes.Root.ReferenceLayerChanges;

internal class ReferenceLayerTopMost_Change : Change
{
    private bool isTopMost;

    [GenerateMakeChangeAction]
    public ReferenceLayerTopMost_Change(bool isTopMost)
    {
        this.isTopMost = isTopMost;
    }

    public override bool InitializeAndValidate(Document target) => true;

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.ReferenceLayer!.IsTopMost = isTopMost;
        ignoreInUndo = false;
        return new ReferenceLayerTopMost_ChangeInfo(isTopMost);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.ReferenceLayer!.IsTopMost = !isTopMost;
        return new ReferenceLayerTopMost_ChangeInfo(!isTopMost);
    }
}
