using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class ClearSelection_Change : Change
{
    private bool originalIsEmpty;
    private CommittedChunkStorage? savedSelection;
    private SKPath? originalPath;
    public override void Initialize(Document target)
    {
        originalIsEmpty = target.Selection.IsEmptyAndInactive;
        if (!originalIsEmpty)
            savedSelection = new(target.Selection.SelectionImage, target.Selection.SelectionImage.FindAllChunks());
        originalPath = new SKPath(target.Selection.SelectionPath);
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        if (originalIsEmpty)
        {
            ignoreInUndo = true;
            return null;
        }
        target.Selection.IsEmptyAndInactive = true;

        target.Selection.SelectionImage.CancelChanges();
        target.Selection.SelectionImage.EnqueueClear();
        HashSet<Vector2i> affChunks = target.Selection.SelectionImage.FindAffectedChunks();
        target.Selection.SelectionImage.CommitChanges();

        target.Selection.SelectionPath.Dispose();
        target.Selection.SelectionPath = new SKPath();

        ignoreInUndo = false;
        return new Selection_ChangeInfo() { Chunks = affChunks };
    }

    public override IChangeInfo? Revert(Document target)
    {
        if (originalIsEmpty)
            return new Selection_ChangeInfo() { Chunks = new() };
        target.Selection.IsEmptyAndInactive = false;

        target.Selection.SelectionImage.CancelChanges();
        savedSelection!.ApplyChunksToImage(target.Selection.SelectionImage);
        HashSet<Vector2i> affChunks = target.Selection.SelectionImage.FindAffectedChunks();
        target.Selection.SelectionImage.CommitChanges();

        target.Selection.SelectionPath.Dispose();
        target.Selection.SelectionPath = new SKPath(originalPath);

        return new Selection_ChangeInfo() { Chunks = affChunks };
    }

    public override void Dispose()
    {
        savedSelection?.Dispose();
        originalPath?.Dispose();
    }
}
