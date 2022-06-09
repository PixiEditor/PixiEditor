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

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);

        SKPath? selection = target.Selection.SelectionPath.IsEmpty ? null : target.Selection.SelectionPath;
        using var floodFilledChunks = FloodFillHelper.FloodFill(image, selection, pos, color);
        (chunkStorage, var affectedChunks) = floodFilledChunks.DrawOnChunkyImage(image);

        ignoreInUndo = false;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affectedChunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affectedChunks = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, ref chunkStorage);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affectedChunks, drawOnMask);
    }

    public override void Dispose()
    {
        chunkStorage?.Dispose();
    }
}
