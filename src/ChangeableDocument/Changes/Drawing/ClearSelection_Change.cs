using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;

namespace ChangeableDocument.Changes.Drawing
{
    internal class ClearSelection_Change : IChange
    {
        private bool originalIsEmpty;
        private CommitedChunkStorage? savedSelection;
        public void Initialize(Document target)
        {
            originalIsEmpty = target.Selection.IsEmptyAndInactive;
            if (!originalIsEmpty)
                savedSelection = new(target.Selection.SelectionImage, target.Selection.SelectionImage.FindAllChunks());
        }

        public IChangeInfo? Apply(Document target)
        {
            if (originalIsEmpty)
                return new Selection_ChangeInfo() { Chunks = new() };
            target.Selection.IsEmptyAndInactive = true;

            target.Selection.SelectionImage.CancelChanges();
            target.Selection.SelectionImage.Clear();
            HashSet<Vector2i> affChunks = target.Selection.SelectionImage.FindAffectedChunks();
            target.Selection.SelectionImage.CommitChanges();

            return new Selection_ChangeInfo() { Chunks = affChunks };
        }

        public IChangeInfo? Revert(Document target)
        {
            if (originalIsEmpty)
                return new Selection_ChangeInfo() { Chunks = new() };
            if (savedSelection == null)
                throw new Exception("No saved selection to restore");
            target.Selection.IsEmptyAndInactive = false;

            target.Selection.SelectionImage.CancelChanges();
            savedSelection.ApplyChunksToImage(target.Selection.SelectionImage);
            HashSet<Vector2i> affChunks = target.Selection.SelectionImage.FindAffectedChunks();
            target.Selection.SelectionImage.CommitChanges();

            return new Selection_ChangeInfo() { Chunks = affChunks };
        }

        public void Dispose()
        {
            savedSelection?.Dispose();
        }
    }
}
