using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class ClearSelection_Change : Change
{
    private CommittedChunkStorage? savedSelection;
    private SKPath? originalPath;

    [GenerateMakeChangeAction]
    public ClearSelection_Change() { }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (target.Selection.IsEmptyAndInactive)
            return new Error();
        savedSelection = new(target.Selection.SelectionImage, target.Selection.SelectionImage.FindAllChunks());
        originalPath = new SKPath(target.Selection.SelectionPath);
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        target.Selection.IsEmptyAndInactive = true;

        target.Selection.SelectionImage.CancelChanges();
        target.Selection.SelectionImage.EnqueueClear();
        HashSet<VecI> affChunks = target.Selection.SelectionImage.FindAffectedChunks();
        target.Selection.SelectionImage.CommitChanges();

        target.Selection.SelectionPath.Dispose();
        target.Selection.SelectionPath = new SKPath();

        ignoreInUndo = false;
        return new Selection_ChangeInfo() { Chunks = affChunks };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.Selection.IsEmptyAndInactive = false;

        target.Selection.SelectionImage.CancelChanges();
        savedSelection!.ApplyChunksToImage(target.Selection.SelectionImage);
        HashSet<VecI> affChunks = target.Selection.SelectionImage.FindAffectedChunks();
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
