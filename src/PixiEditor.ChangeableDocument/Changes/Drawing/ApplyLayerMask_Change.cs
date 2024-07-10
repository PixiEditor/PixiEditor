using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class ApplyLayerMask_Change : Change
{
    private readonly Guid layerGuid;
    private CommittedChunkStorage? savedMask;
    private CommittedChunkStorage? savedLayer;
    private int frame;

    [GenerateMakeChangeAction]
    public ApplyLayerMask_Change(Guid layerGuid, int frame)
    {
        this.layerGuid = layerGuid;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        //TODO: Check if support for different Layer types is needed here.
        if (!target.TryFindMember<ImageLayerNode>(layerGuid, out var layer) || layer.Mask.Value is null)
            return false;

        var layerImage = layer.GetLayerImageAtFrame(frame);
        savedLayer = new CommittedChunkStorage(layerImage, layerImage.FindCommittedChunks());
        savedMask = new CommittedChunkStorage(layer.Mask.Value, layer.Mask.Value.FindCommittedChunks());
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var layer = target.FindMemberOrThrow<ImageLayerNode>(layerGuid);
        if (layer.Mask is null)
            throw new InvalidOperationException("Cannot apply layer mask, no mask");

        var layerImage = layer.GetLayerImageAtFrame(frame);
        ChunkyImage newLayerImage = new ChunkyImage(target.Size);
        newLayerImage.AddRasterClip(layer.Mask.Value);
        newLayerImage.EnqueueDrawCommitedChunkyImage(VecI.Zero, layerImage);
        newLayerImage.CommitChanges();

        var affectedChunks = layerImage.FindAllChunks();
        // use a temp value to ensure that LayerImage always stays in a valid state
        var toDispose = layerImage;
        layer.SetLayerImageAtFrame(frame, newLayerImage);
        toDispose.Dispose();

        var toDisposeMask = layer.Mask.NonOverridenValue;
        layer.Mask.NonOverridenValue = null;
        toDisposeMask.Dispose();

        ignoreInUndo = false;
        return new List<IChangeInfo>
        {
            new StructureMemberMask_ChangeInfo(layerGuid, false),
            new LayerImageArea_ChangeInfo(layerGuid, new AffectedArea(affectedChunks))
        };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var layer = target.FindMemberOrThrow<ImageLayerNode>(layerGuid);
        if (layer.Mask is not null)
            throw new InvalidOperationException("Cannot restore layer mask, it already has one");
        if (savedLayer is null || savedMask is null)
            throw new InvalidOperationException("Cannot restore layer mask, no saved data");

        ChunkyImage newMask = new ChunkyImage(target.Size);
        savedMask.ApplyChunksToImage(newMask);
        var affectedChunksMask = newMask.FindAffectedArea();
        newMask.CommitChanges();
        layer.Mask.NonOverridenValue = newMask;

        var layerImage = layer.GetLayerImageAtFrame(frame);
        
        savedLayer.ApplyChunksToImage(layerImage);
        var affectedChunksLayer = layerImage.FindAffectedArea();
        layerImage.CommitChanges();

        return new List<IChangeInfo>
        {
            new StructureMemberMask_ChangeInfo(layerGuid, true),
            new LayerImageArea_ChangeInfo(layerGuid, affectedChunksLayer),
            new MaskArea_ChangeInfo(layerGuid, affectedChunksMask)
        };
    }

    public override void Dispose()
    {
        savedLayer?.Dispose();
        savedMask?.Dispose();
    }
}
