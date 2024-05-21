using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class CombineStructureMembersOnto_Change : Change
{
    private HashSet<Guid> membersToMerge;

    private HashSet<Guid> layersToCombine = new();

    private Guid targetLayer;
    private CommittedChunkStorage? originalChunks;

    [GenerateMakeChangeAction]
    public CombineStructureMembersOnto_Change(HashSet<Guid> membersToMerge, Guid targetLayer)
    {
        this.membersToMerge = new HashSet<Guid>(membersToMerge);
        this.targetLayer = targetLayer;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.HasMember(targetLayer) || membersToMerge.Count == 0)
            return false;
        foreach (Guid guid in membersToMerge)
        {
            if (!target.TryFindMember(guid, out var member))
                return false;
            
            if (member is Layer layer)
                layersToCombine.Add(layer.GuidValue);
            else if (member is Folder innerFolder)
                AddChildren(innerFolder, layersToCombine);
        }

        return true;
    }

    private void AddChildren(Folder folder, HashSet<Guid> collection)
    {
        foreach (var child in folder.Children)
        {
            if (child is Layer layer)
                collection.Add(layer.GuidValue);
            else if (child is Folder innerFolder)
                AddChildren(innerFolder, collection);
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        //TODO: Add support for different Layer types
        var toDrawOn = target.FindMemberOrThrow<RasterLayer>(targetLayer);

        var chunksToCombine = new HashSet<VecI>();
        foreach (var guid in layersToCombine)
        {
            var layer = target.FindMemberOrThrow<RasterLayer>(guid);
            chunksToCombine.UnionWith(layer.LayerImage.FindAllChunks());
        }

        toDrawOn.LayerImage.EnqueueClear();
        foreach (var chunk in chunksToCombine)
        {
            OneOf<Chunk, EmptyChunk> combined = ChunkRenderer.MergeChosenMembers(chunk, ChunkResolution.Full, target.StructureRoot, layersToCombine);
            if (combined.IsT0)
            {
                toDrawOn.LayerImage.EnqueueDrawImage(chunk * ChunkyImage.FullChunkSize, combined.AsT0.Surface);
                combined.AsT0.Dispose();
            }
        }
        var affArea = toDrawOn.LayerImage.FindAffectedArea();
        originalChunks = new CommittedChunkStorage(toDrawOn.LayerImage, affArea.Chunks);
        toDrawOn.LayerImage.CommitChanges();

        ignoreInUndo = false;
        return new LayerImageArea_ChangeInfo(targetLayer, affArea);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var toDrawOn = target.FindMemberOrThrow<RasterLayer>(targetLayer);
        var affectedArea = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(toDrawOn.LayerImage, ref originalChunks);
        return new LayerImageArea_ChangeInfo(targetLayer, affectedArea);
    }

    public override void Dispose()
    {
        originalChunks?.Dispose();
    }
}
