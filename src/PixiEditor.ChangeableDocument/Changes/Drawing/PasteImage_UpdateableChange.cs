using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class PasteImage_UpdateableChange : UpdateableChange
{
    private ShapeCorners corners;
    private readonly Guid memberGuid;
    private readonly bool drawOnMask;
    private readonly Surface imageToPaste;
    private CommittedChunkStorage? savedChunks;

    public PasteImage_UpdateableChange(ShapeCorners corners, Surface imageToPaste, Guid memberGuid, bool drawOnMask)
    {
        this.corners = corners;
        this.memberGuid = memberGuid;
        this.drawOnMask = drawOnMask;
        this.imageToPaste = new Surface(imageToPaste);
    }

    public void Update(ShapeCorners newCorners)
    {
        corners = newCorners;
    }

    private HashSet<Vector2i> DrawImage(ChunkyImage targetImage)
    {
        var prevChunks = targetImage.FindAffectedChunks();

        targetImage.CancelChanges();
        targetImage.EnqueueDrawImage(corners, imageToPaste);

        var affectedChunks = targetImage.FindAffectedChunks();
        affectedChunks.UnionWith(prevChunks);
        return affectedChunks;
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImage(target, memberGuid, drawOnMask);
        var chunks = DrawImage(targetImage);
        savedChunks?.Dispose();
        savedChunks = new(targetImage, targetImage.FindAffectedChunks());
        targetImage.CommitChanges();
        ignoreInUndo = false;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, chunks, drawOnMask);
    }

    public override IChangeInfo? ApplyTemporarily(Document target)
    {
        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImage(target, memberGuid, drawOnMask);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, DrawImage(targetImage), drawOnMask);
    }

    public override IChangeInfo? Revert(Document target)
    {
        if (savedChunks is null)
            throw new InvalidOperationException("No saved chunks to restore");
        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImage(target, memberGuid, drawOnMask);
        savedChunks.ApplyChunksToImage(targetImage);
        var chunks = targetImage.FindAffectedChunks();
        targetImage.CommitChanges();
        savedChunks.Dispose();
        savedChunks = null;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, chunks, drawOnMask);
    }

    public override void Dispose()
    {
        imageToPaste.Dispose();
        savedChunks?.Dispose();
    }
}
