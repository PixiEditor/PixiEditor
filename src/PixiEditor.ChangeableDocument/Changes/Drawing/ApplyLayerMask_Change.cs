using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class ApplyLayerMask_Change : Change
{
    private readonly Guid layerGuid;
    private CommittedChunkStorage? savedMask;
    private CommittedChunkStorage? savedLayer;

    [GenerateMakeChangeAction]
    public ApplyLayerMask_Change(Guid layerGuid)
    {
        this.layerGuid = layerGuid;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        var member = target.FindMember(layerGuid);
        if (member is not Layer layer || layer.Mask is null)
            return new Error();

        savedLayer = new CommittedChunkStorage(layer.LayerImage, layer.LayerImage.FindCommittedChunks());
        savedMask = new CommittedChunkStorage(layer.Mask, layer.Mask.FindCommittedChunks());
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var layer = (Layer)target.FindMemberOrThrow(layerGuid);
        if (layer.Mask is null)
            throw new InvalidOperationException("Cannot apply layer mask, no mask");

        ChunkyImage newLayerImage = new ChunkyImage(target.Size);
        newLayerImage.AddRasterClip(layer.Mask);
        newLayerImage.EnqueueDrawChunkyImage(VecI.Zero, layer.LayerImage);
        newLayerImage.CommitChanges();

        var affectedChunks = layer.LayerImage.FindAllChunks();
        // use a temp value to ensure that LayerImage always stays in a valid state
        var toDispose = layer.LayerImage;
        layer.LayerImage = newLayerImage;
        toDispose.Dispose();

        var toDisposeMask = layer.Mask;
        layer.Mask = null;
        toDisposeMask.Dispose();

        ignoreInUndo = false;
        return new List<IChangeInfo>
        {
            new StructureMemberMask_ChangeInfo(layerGuid, false),
            new LayerImageChunks_ChangeInfo(layerGuid, affectedChunks)
        };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var layer = (Layer)target.FindMemberOrThrow(layerGuid);
        if (layer.Mask is not null)
            throw new InvalidOperationException("Cannot restore layer mask, it already has one");
        if (savedLayer is null || savedMask is null)
            throw new InvalidOperationException("Cannot restore layer mask, no saved data");

        ChunkyImage newMask = new ChunkyImage(target.Size);
        savedMask.ApplyChunksToImage(newMask);
        var affectedChunksMask = newMask.FindAffectedChunks();
        newMask.CommitChanges();
        layer.Mask = newMask;

        savedLayer.ApplyChunksToImage(layer.LayerImage);
        var affectedChunksLayer = layer.LayerImage.FindAffectedChunks();
        layer.LayerImage.CommitChanges();

        return new List<IChangeInfo>
        {
            new StructureMemberMask_ChangeInfo(layerGuid, true),
            new LayerImageChunks_ChangeInfo(layerGuid, affectedChunksLayer),
            new MaskChunks_ChangeInfo(layerGuid, affectedChunksMask)
        };
    }

    public override void Dispose()
    {
        savedLayer?.Dispose();
        savedMask?.Dispose();
    }
}
