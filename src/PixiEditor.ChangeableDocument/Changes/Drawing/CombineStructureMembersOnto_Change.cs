using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class CombineStructureMembersOnto_Change : Change
{
    private HashSet<Guid> membersToMerge;

    private HashSet<Guid> layersToCombine = new();

    private Guid targetLayer;
    private CommittedChunkStorage? originalChunks;

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

        var chunksToCombine = new HashSet<Vector2i>();
        foreach (var guid in layersToCombine)
        {
            var layer = (Layer)target.FindMemberOrThrow(guid);
            chunksToCombine.UnionWith(layer.LayerImage.FindAllChunks());
        }

        toDrawOn.LayerImage.Clear();
        foreach (var chunk in chunksToCombine)
        {
            using var combined = ChunkRenderer.RenderSpecificLayers(chunk, ChunkResolution.Full, target.StructureRoot, layersToCombine);
            toDrawOn.LayerImage.DrawImage(chunk * ChunkyImage.ChunkSize, combined.Surface);
        }
        var affectedChunks = toDrawOn.LayerImage.FindAffectedChunks();
        originalChunks = new CommittedChunkStorage(toDrawOn.LayerImage, affectedChunks);
        toDrawOn.LayerImage.CommitChanges();

        ignoreInUndo = false;
        return new LayerImageChunks_ChangeInfo()
        {
            LayerGuid = targetLayer,
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
            LayerGuid = targetLayer,
            Chunks = affectedChunks
        };
    }

    public override void Dispose()
    {
        originalChunks?.Dispose();
    }
}
