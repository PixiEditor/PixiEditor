using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class SelectRectangle_UpdateableChange : UpdateableChange
{
    private bool originalIsEmpty;
    private Vector2i pos;
    private Vector2i size;
    private CommittedChunkStorage? originalSelectionState;
    public SelectRectangle_UpdateableChange(Vector2i pos, Vector2i size)
    {
        Update(pos, size);
    }
    public override void Initialize(Document target)
    {
        originalIsEmpty = target.Selection.IsEmptyAndInactive;
    }

    public void Update(Vector2i pos, Vector2i size)
    {
        this.pos = pos;
        this.size = size;
    }

    public override IChangeInfo? ApplyTemporarily(Document target)
    {
        var oldChunks = target.Selection.SelectionImage.FindAffectedChunks();
        target.Selection.SelectionImage.CancelChanges();
        target.Selection.IsEmptyAndInactive = false;
        target.Selection.SelectionImage.EnqueueDrawRectangle(new ShapeData(pos, size, 0, SKColors.Transparent, Selection.SelectionColor));

        oldChunks.UnionWith(target.Selection.SelectionImage.FindAffectedChunks());
        return new Selection_ChangeInfo() { Chunks = oldChunks };
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        var changes = ApplyTemporarily(target);
        originalSelectionState = new CommittedChunkStorage(target.Selection.SelectionImage, ((Selection_ChangeInfo)changes!).Chunks!);
        target.Selection.SelectionImage.CommitChanges();
        target.Selection.IsEmptyAndInactive = target.Selection.SelectionImage.CheckIfCommittedIsEmpty();
        ignoreInUndo = false;
        return changes;
    }

    public override IChangeInfo? Revert(Document target)
    {
        target.Selection.IsEmptyAndInactive = originalIsEmpty;
        originalSelectionState!.ApplyChunksToImage(target.Selection.SelectionImage);
        originalSelectionState.Dispose();
        originalSelectionState = null;
        var changes = new Selection_ChangeInfo() { Chunks = target.Selection.SelectionImage.FindAffectedChunks() };
        target.Selection.SelectionImage.CommitChanges();
        return changes;
    }

    public override void Dispose()
    {
        originalSelectionState?.Dispose();
    }
}
