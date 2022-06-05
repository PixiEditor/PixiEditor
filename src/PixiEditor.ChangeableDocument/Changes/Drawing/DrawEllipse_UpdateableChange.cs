using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class DrawEllipse_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private RectI location;
    private readonly SKColor strokeColor;
    private readonly SKColor fillColor;
    private readonly int strokeWidth;
    private readonly bool drawOnMask;

    private CommittedChunkStorage? storedChunks;

    [GenerateUpdateableChangeActions]
    public DrawEllipse_UpdateableChange(Guid memberGuid, RectI location, SKColor strokeColor, SKColor fillColor, int strokeWidth, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.location = location;
        this.strokeColor = strokeColor;
        this.fillColor = fillColor;
        this.strokeWidth = strokeWidth;
        this.drawOnMask = drawOnMask;
    }

    [UpdateChangeMethod]
    public void Update(RectI location)
    {
        this.location = location;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return new Error();
        return new Success();
    }

    private HashSet<VecI> UpdateEllipse(Document target, ChunkyImage targetImage)
    {
        var oldAffectedChunks = targetImage.FindAffectedChunks();

        targetImage.CancelChanges();
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, targetImage, memberGuid, drawOnMask);
        targetImage.EnqueueDrawEllipse(location, strokeColor, fillColor, strokeWidth);

        var affectedChunks = targetImage.FindAffectedChunks();
        affectedChunks.UnionWith(oldAffectedChunks);

        return affectedChunks;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        var chunks = UpdateEllipse(target, image);
        storedChunks = new CommittedChunkStorage(image, image.FindAffectedChunks());
        image.CommitChanges();
        ignoreInUndo = false;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, chunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        var chunks = UpdateEllipse(target, image);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, chunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        storedChunks!.ApplyChunksToImage(targetImage);
        storedChunks.Dispose();
        storedChunks = null;

        var changes = DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, targetImage.FindAffectedChunks(), drawOnMask);
        targetImage.CommitChanges();
        return changes;
    }

    public override void Dispose()
    {
        storedChunks?.Dispose();
    }
}
