using PixiEditor.ChangeableDocument.Rendering;

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

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (target.FindMember(targetLayer) is null || membersToMerge.Count == 0)
            return new Error();
        foreach (Guid guid in membersToMerge)
        {
            var member = target.FindMember(guid);
            if (member is null)
                return new Error();
            if (member is Layer layer)
                layersToCombine.Add(layer.GuidValue);
            else if (member is Folder innerFolder)
                AddChildren(innerFolder, layersToCombine);
        }
        return new Success();
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

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        Layer toDrawOn = (Layer)target.FindMemberOrThrow(targetLayer);

        var chunksToCombine = new HashSet<VecI>();
        foreach (var guid in layersToCombine)
        {
            var layer = (Layer)target.FindMemberOrThrow(guid);
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
        var affectedChunks = toDrawOn.LayerImage.FindAffectedChunks();
        originalChunks = new CommittedChunkStorage(toDrawOn.LayerImage, affectedChunks);
        toDrawOn.LayerImage.CommitChanges();

        ignoreInUndo = false;
        return new LayerImageChunks_ChangeInfo(targetLayer, affectedChunks);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Layer toDrawOn = (Layer)target.FindMemberOrThrow(targetLayer);
        var affectedChunks = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(toDrawOn.LayerImage, ref originalChunks);
        return new LayerImageChunks_ChangeInfo(targetLayer, affectedChunks);
    }

    public override void Dispose()
    {
        originalChunks?.Dispose();
    }
}
