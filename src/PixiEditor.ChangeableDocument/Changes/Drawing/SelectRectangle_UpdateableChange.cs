using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class SelectRectangle_UpdateableChange : UpdateableChange
{
    private bool originalIsEmpty;
    private VecI pos;
    private VecI size;
    private CommittedChunkStorage? originalSelectionState;
    private SKPath? originalPath;

    [GenerateUpdateableChangeActions]
    public SelectRectangle_UpdateableChange(VecI pos, VecI size)
    {
        Update(pos, size);
    }
    public override void Initialize(Document target)
    {
        originalIsEmpty = target.Selection.IsEmptyAndInactive;
        originalPath = new SKPath(target.Selection.SelectionPath);
    }

    [UpdateChangeMethod]
    public void Update(VecI pos, VecI size)
    {
        this.pos = pos;
        this.size = size;
    }

    public override IChangeInfo? ApplyTemporarily(Document target)
    {
        var oldChunks = target.Selection.SelectionImage.FindAffectedChunks();
        target.Selection.SelectionImage.CancelChanges();
        target.Selection.IsEmptyAndInactive = false;
        target.Selection.SelectionImage.EnqueueDrawRectangle(new ShapeData(pos + size / 2, size, 0, 0, SKColors.Transparent, Selection.SelectionColor));

        using SKPath rect = new SKPath();
        rect.MoveTo(pos);
        rect.LineTo(pos.X + size.X, pos.Y);
        rect.LineTo(pos + size);
        rect.LineTo(pos.X, pos.Y + size.Y);
        rect.LineTo(pos);

        target.Selection.SelectionPath.Dispose();
        target.Selection.SelectionPath = originalPath!.Op(rect, SKPathOp.Union);

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
        target.Selection.SelectionPath.Dispose();
        target.Selection.SelectionPath = new SKPath(originalPath);
        return changes;
    }

    public override void Dispose()
    {
        originalSelectionState?.Dispose();
        originalPath?.Dispose();
    }
}
