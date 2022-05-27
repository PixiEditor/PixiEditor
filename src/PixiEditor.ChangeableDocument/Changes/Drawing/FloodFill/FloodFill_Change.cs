using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;
internal class FloodFill_Change : Change
{
    private readonly Guid memberGuid;
    private readonly VecI pos;
    private readonly SKColor color;
    private readonly bool drawOnMask;
    private CommittedChunkStorage? chunkStorage = null;

    [GenerateMakeChangeAction]
    public FloodFill_Change(Guid memberGuid, VecI pos, SKColor color, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.pos = pos;
        this.color = color;
        this.drawOnMask = drawOnMask;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (pos.X < 0 || pos.Y < 0 || pos.X >= target.Size.X || pos.Y >= target.Size.X)
            return new Error();
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return new Error();
        return new Success();
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);

        using var floodFilledChunks = FloodFillHelper.FloodFill(image, pos, color);
        (chunkStorage, var affectedChunks) = floodFilledChunks.DrawOnChunkyImage(image);

        ignoreInUndo = false;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affectedChunks, drawOnMask);
    }

    public override IChangeInfo? Revert(Document target)
    {
        if (chunkStorage is null)
            throw new InvalidOperationException("No saved chunks to revert to");
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        chunkStorage.ApplyChunksToImage(image);
        var affectedChunks = image.FindAffectedChunks();
        image.CommitChanges();
        chunkStorage.Dispose();
        chunkStorage = null;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affectedChunks, drawOnMask);
    }

    public override void Dispose()
    {
        chunkStorage?.Dispose();
    }
}
