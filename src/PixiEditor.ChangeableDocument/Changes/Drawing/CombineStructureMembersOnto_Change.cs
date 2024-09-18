using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.Structure;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class CombineStructureMembersOnto_Change : Change
{
    private HashSet<Guid> membersToMerge;

    private HashSet<Guid> layersToCombine = new();
    private int frame;

    private Guid targetLayer;
    private CommittedChunkStorage? originalChunks;
    

    [GenerateMakeChangeAction]
    public CombineStructureMembersOnto_Change(HashSet<Guid> membersToMerge, Guid targetLayer, int frame)
    {
        this.membersToMerge = new HashSet<Guid>(membersToMerge);
        this.targetLayer = targetLayer;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.HasMember(targetLayer) || membersToMerge.Count == 0)
            return false;
        foreach (Guid guid in membersToMerge)
        {
            if (!target.TryFindMember(guid, out var member))
                return false;

            if (member is LayerNode layer)
                layersToCombine.Add(layer.Id);
            else if (member is FolderNode innerFolder)
                AddChildren(innerFolder, layersToCombine);
        }

        return true;
    }

    private void AddChildren(FolderNode folder, HashSet<Guid> collection)
    {
        if (folder.Content.Connection != null)
        {
            folder.Content.Connection.Node.TraverseBackwards(node =>
            {
                if (node is LayerNode layer)
                {
                    collection.Add(layer.Id);
                    return true;
                }

                return true;
            });
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        // TODO: add merging similar layers (vector -> vector)
        var toDrawOn = target.FindMemberOrThrow<LayerNode>(targetLayer);

        var chunksToCombine = new HashSet<VecI>();
        foreach (var guid in layersToCombine)
        {
            var layer = target.FindMemberOrThrow<LayerNode>(guid);
            if(layer is not IRasterizable or ImageLayerNode)
                continue;

            if (layer is ImageLayerNode imageLayerNode)
            {
                var layerImage = imageLayerNode.GetLayerImageAtFrame(frame);
                chunksToCombine.UnionWith(layerImage.FindAllChunks());
            }
            else
            {
                AddChunksByTightBounds(layer, chunksToCombine);
            }
        }
        
        List<IChangeInfo> changes = new();
        
        var toDrawOnImage = ((ImageLayerNode)toDrawOn).GetLayerImageAtFrame(frame);
        toDrawOnImage.EnqueueClear();

        DocumentRenderer renderer = new(target);

        AffectedArea affArea = new();
        DrawingBackendApi.Current.RenderingDispatcher.Invoke(() =>
        {
            RectI? globalClippingRect = new RectI(0, 0, target.Size.X, target.Size.Y);
            foreach (var chunk in chunksToCombine)
            {
                OneOf<Chunk, EmptyChunk> combined =
                    renderer.RenderLayersChunk(chunk, ChunkResolution.Full, frame, layersToCombine, globalClippingRect);
                if (combined.IsT0)
                {
                    toDrawOnImage.EnqueueDrawImage(chunk * ChunkyImage.FullChunkSize, combined.AsT0.Surface);
                    combined.AsT0.Dispose();
                }
            }

            affArea = toDrawOnImage.FindAffectedArea();
            originalChunks = new CommittedChunkStorage(toDrawOnImage, affArea.Chunks);
            toDrawOnImage.CommitChanges();
        });


        ignoreInUndo = false;
        
        changes.Add(new LayerImageArea_ChangeInfo(targetLayer, affArea));
        return changes;
    }

    private void AddChunksByTightBounds(LayerNode layer, HashSet<VecI> chunksToCombine)
    {
        var tightBounds = layer.GetTightBounds(frame);
        if (tightBounds.HasValue)
        {
            VecI chunk = (VecI)tightBounds.Value.TopLeft / ChunkyImage.FullChunkSize;
            VecI sizeInChunks = ((VecI)tightBounds.Value.Size / ChunkyImage.FullChunkSize);
            sizeInChunks = new VecI(Math.Max(1, sizeInChunks.X), Math.Max(1, sizeInChunks.Y));
            for (int x = 0; x < sizeInChunks.X; x++)
            {
                for (int y = 0; y < sizeInChunks.Y; y++)
                {
                    chunksToCombine.Add(chunk + new VecI(x, y));
                }
            }
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var toDrawOn = target.FindMemberOrThrow<ImageLayerNode>(targetLayer);
        var affectedArea =
            DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(toDrawOn.GetLayerImageAtFrame(frame),
                ref originalChunks);
        
        List<IChangeInfo> changes = new();
        changes.Add(new LayerImageArea_ChangeInfo(targetLayer, affectedArea));

        return changes; 
    }

    public override void Dispose()
    {
        originalChunks?.Dispose();
    }
}
