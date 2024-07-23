using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
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

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        //TODO: Add support for different Layer types
        var toDrawOn = target.FindMemberOrThrow<ImageLayerNode>(targetLayer);

        var chunksToCombine = new HashSet<VecI>();
        foreach (var guid in layersToCombine)
        {
            var layer = target.FindMemberOrThrow<ImageLayerNode>(guid);
            var layerImage = layer.GetLayerImageAtFrame(frame);
            chunksToCombine.UnionWith(layerImage.FindAllChunks());
        }

        var toDrawOnImage = toDrawOn.GetLayerImageAtFrame(frame);
        toDrawOnImage.EnqueueClear();
        
        DocumentRenderer renderer = new(target);
        
        foreach (var chunk in chunksToCombine)
        {
            OneOf<Chunk, EmptyChunk> combined = renderer.RenderLayersChunk(chunk, ChunkResolution.Full, frame, layersToCombine);
            if (combined.IsT0)
            {
                toDrawOnImage.EnqueueDrawImage(chunk * ChunkyImage.FullChunkSize, combined.AsT0.Surface);
                combined.AsT0.Dispose();
            }
        }
        var affArea = toDrawOnImage.FindAffectedArea();
        originalChunks = new CommittedChunkStorage(toDrawOnImage, affArea.Chunks);
        toDrawOnImage.CommitChanges();

        ignoreInUndo = false;
        return new LayerImageArea_ChangeInfo(targetLayer, affArea);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var toDrawOn = target.FindMemberOrThrow<ImageLayerNode>(targetLayer);
        var affectedArea = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(toDrawOn.GetLayerImageAtFrame(frame), ref originalChunks);
        return new LayerImageArea_ChangeInfo(targetLayer, affectedArea);
    }

    public override void Dispose()
    {
        originalChunks?.Dispose();
    }
}
