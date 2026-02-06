using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

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
        if (!target.TryFindMember<ImageLayerNode>(layerGuid, out var layer) || layer.EmbeddedMask is null)
            return false;

        var layerImage = layer.GetLayerImageAtFrame(frame);
        if (layerImage is null)
            return false;

        savedLayer = new CommittedChunkStorage(layerImage, layerImage.FindCommittedChunks());
        savedMask = new CommittedChunkStorage(layer.EmbeddedMask, layer.EmbeddedMask.FindCommittedChunks());
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        var layer = target.FindMemberOrThrow<ImageLayerNode>(layerGuid);
        if (layer.EmbeddedMask is null)
            throw new InvalidOperationException("Cannot apply layer mask, no mask");

        var layerImage = layer.GetLayerImageAtFrame(frame);
        if (layerImage is null)
            throw new InvalidOperationException("Cannot apply layer mask, no layer image at frame");

        ChunkyImage newLayerImage = new ChunkyImage(target.Size, target.ProcessingColorSpace);
        newLayerImage.AddRasterClip(layer.EmbeddedMask);
        newLayerImage.EnqueueDrawCommitedChunkyImage(VecI.Zero, layerImage);
        newLayerImage.CommitChanges();

        var affectedChunks = layerImage.FindAllChunks();
        // use a temp value to ensure that LayerImage always stays in a valid state
        var toDispose = layerImage;
        layer.SetLayerImageAtFrame(frame, newLayerImage);
        toDispose.Dispose();

        var toDisposeMask = layer.EmbeddedMask;
        layer.EmbeddedMask = null;
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
        if (layer.EmbeddedMask is not null)
            throw new InvalidOperationException("Cannot restore layer mask, it already has one");
        if (savedLayer is null || savedMask is null)
            throw new InvalidOperationException("Cannot restore layer mask, no saved data");

        ChunkyImage newMask = new ChunkyImage(target.Size, target.ProcessingColorSpace);
        savedMask.ApplyChunksToImage(newMask);
        var affectedChunksMask = newMask.FindAffectedArea();
        newMask.CommitChanges();
        layer.EmbeddedMask = newMask;

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
