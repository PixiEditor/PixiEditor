using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class ResizeCanvas_Change : Change
{
    private VecI originalSize;
    private int originalHorAxisY;
    private int originalVerAxisX;
    private Dictionary<Guid, CommittedChunkStorage> deletedChunks = new();
    private Dictionary<Guid, CommittedChunkStorage> deletedMaskChunks = new();
    private CommittedChunkStorage? selectionChunkStorage;
    private VecI newSize;

    [GenerateMakeChangeAction]
    public ResizeCanvas_Change(VecI size)
    {
        newSize = size;
    }
    public override void Initialize(Document target)
    {
        originalSize = target.Size;
        originalHorAxisY = target.HorizontalSymmetryAxisY;
        originalVerAxisX = target.VerticalSymmetryAxisX;
    }

    private void ForEachLayer(Folder folder, Action<Layer> action)
    {
        foreach (var child in folder.Children)
        {
            if (child is Layer layer)
            {
                action(layer);
            }
            else if (child is Folder innerFolder)
                ForEachLayer(innerFolder, action);
        }
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        if (originalSize == newSize)
        {
            ignoreInUndo = true;
            return null;
        }

        target.Size = newSize;
        target.VerticalSymmetryAxisX = Math.Clamp(originalVerAxisX, 0, target.Size.X);
        target.HorizontalSymmetryAxisY = Math.Clamp(originalHorAxisY, 0, target.Size.Y);

        ForEachLayer(target.StructureRoot, (layer) =>
        {
            layer.LayerImage.EnqueueResize(newSize);
            deletedChunks.Add(layer.GuidValue, new CommittedChunkStorage(layer.LayerImage, layer.LayerImage.FindAffectedChunks()));
            layer.LayerImage.CommitChanges();

            if (layer.Mask is null)
                return;

            layer.Mask.EnqueueResize(newSize);
            deletedMaskChunks.Add(layer.GuidValue, new CommittedChunkStorage(layer.Mask, layer.Mask.FindAffectedChunks()));
            layer.Mask.CommitChanges();
        });

        target.Selection.SelectionImage.EnqueueResize(newSize);
        selectionChunkStorage = new(target.Selection.SelectionImage, target.Selection.SelectionImage.FindAffectedChunks());
        target.Selection.SelectionImage.CommitChanges();

        ignoreInUndo = false;
        return new Size_ChangeInfo();
    }

    public override IChangeInfo? Revert(Document target)
    {
        if (originalSize == newSize)
            return null;

        target.Size = originalSize;
        ForEachLayer(target.StructureRoot, (layer) =>
        {
            layer.LayerImage.EnqueueResize(originalSize);
            deletedChunks[layer.GuidValue].ApplyChunksToImage(layer.LayerImage);
            layer.LayerImage.CommitChanges();

            if (layer.Mask is null)
                return;

            layer.Mask.EnqueueResize(originalSize);
            deletedMaskChunks[layer.GuidValue].ApplyChunksToImage(layer.Mask);
            layer.Mask.CommitChanges();
        });

        target.Selection.SelectionImage.EnqueueResize(originalSize);
        selectionChunkStorage!.ApplyChunksToImage(target.Selection.SelectionImage);
        target.Selection.SelectionImage.CommitChanges();
        selectionChunkStorage.Dispose();
        selectionChunkStorage = null;

        target.HorizontalSymmetryAxisY = originalHorAxisY;
        target.VerticalSymmetryAxisX = originalVerAxisX;

        foreach (var stored in deletedChunks)
            stored.Value.Dispose();
        deletedChunks = new();

        return new Size_ChangeInfo();
    }

    public override void Dispose()
    {
        foreach (var layer in deletedChunks)
            layer.Value.Dispose();
        foreach (var mask in deletedMaskChunks)
            mask.Value.Dispose();
        selectionChunkStorage?.Dispose();
    }
}
