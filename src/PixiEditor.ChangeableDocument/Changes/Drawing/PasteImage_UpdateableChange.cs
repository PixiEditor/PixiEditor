using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class PasteImage_UpdateableChange : UpdateableChange
{
    private ShapeCorners corners;
    private readonly Guid memberGuid;
    private readonly bool drawOnMask;
    private readonly Surface imageToPaste;
    private CommittedChunkStorage? savedChunks;
    private static SKPaint RegularPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.SrcOver };

    private bool hasEnqueudImage = false;

    [GenerateUpdateableChangeActions]
    public PasteImage_UpdateableChange(Surface image, ShapeCorners corners, Guid memberGuid, bool isDrawingOnMask)
    {
        this.corners = corners;
        this.memberGuid = memberGuid;
        this.drawOnMask = isDrawingOnMask;
        this.imageToPaste = new Surface(image);
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return new Error();
        return new Success();
    }

    [UpdateChangeMethod]
    public void Update(ShapeCorners corners)
    {
        this.corners = corners;
    }

    private HashSet<VecI> DrawImage(Document target, ChunkyImage targetImage)
    {
        var prevChunks = targetImage.FindAffectedChunks();

        targetImage.CancelChanges();
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, targetImage, memberGuid, drawOnMask);
        targetImage.EnqueueDrawImage(corners, imageToPaste, RegularPaint, false);
        hasEnqueudImage = true;

        var affectedChunks = targetImage.FindAffectedChunks();
        affectedChunks.UnionWith(prevChunks);
        return affectedChunks;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        var chunks = DrawImage(target, targetImage);
        savedChunks?.Dispose();
        savedChunks = new(targetImage, targetImage.FindAffectedChunks());
        targetImage.CommitChanges();
        hasEnqueudImage = false;
        ignoreInUndo = false;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, chunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, DrawImage(target, targetImage), drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (savedChunks is null)
            throw new InvalidOperationException("No saved chunks to restore");
        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        savedChunks.ApplyChunksToImage(targetImage);
        var chunks = targetImage.FindAffectedChunks();
        targetImage.CommitChanges();
        hasEnqueudImage = false;
        savedChunks.Dispose();
        savedChunks = null;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, chunks, drawOnMask);
    }

    public override void Dispose()
    {
        if (hasEnqueudImage)
            throw new InvalidOperationException("Attempted to dispose the change while it's internally stored image is still used enqueued in some ChunkyImage. Most likely someone tried to dispose a change after ApplyTemporarily was called but before the subsequent call to Apply. Don't do that.");
        imageToPaste.Dispose();
        savedChunks?.Dispose();
    }
}
