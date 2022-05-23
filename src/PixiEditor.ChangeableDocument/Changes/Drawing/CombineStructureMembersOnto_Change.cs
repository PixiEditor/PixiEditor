using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using OneOf;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
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

    public override void Initialize(Document target)
    {
        foreach (Guid guid in membersToMerge)
        {
            var member = target.FindMemberOrThrow(guid);
            if (member is Layer layer)
                layersToCombine.Add(layer.GuidValue);
            else if (member is Folder innerFolder)
                AddChildren(innerFolder, layersToCombine);
        }
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

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
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
                toDrawOn.LayerImage.EnqueueDrawImage(chunk * ChunkyImage.ChunkSize, combined.AsT0.Surface);
                combined.AsT0.Surface.Dispose();
            }
        }
        var affectedChunks = toDrawOn.LayerImage.FindAffectedChunks();
        originalChunks = new CommittedChunkStorage(toDrawOn.LayerImage, affectedChunks);
        toDrawOn.LayerImage.CommitChanges();

        ignoreInUndo = false;
        return new LayerImageChunks_ChangeInfo()
        {
            GuidValue = targetLayer,
            Chunks = affectedChunks
        };
    }

    public override IChangeInfo? Revert(Document target)
    {
        Layer toDrawOn = (Layer)target.FindMemberOrThrow(targetLayer);

        originalChunks!.ApplyChunksToImage(toDrawOn.LayerImage);
        var affectedChunks = toDrawOn.LayerImage.FindAffectedChunks();
        toDrawOn.LayerImage.CommitChanges();

        originalChunks.Dispose();
        originalChunks = null;

        return new LayerImageChunks_ChangeInfo()
        {
            GuidValue = targetLayer,
            Chunks = affectedChunks
        };
    }

    public override void Dispose()
    {
        originalChunks?.Dispose();
    }
}
