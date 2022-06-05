using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class ClearSelectedArea_Change : Change
{
    private readonly Guid memberGuid;
    private readonly bool drawOnMask;
    private CommittedChunkStorage? savedChunks;

    [GenerateMakeChangeAction]
    public ClearSelectedArea_Change(Guid memberGuid, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.drawOnMask = drawOnMask;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (target.Selection.SelectionPath.IsEmpty)
            return new Error();
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return new Error();
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        if (savedChunks is not null)
            throw new InvalidOperationException("trying to save chunks while they are already saved");

        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);

        RectD bounds = target.Selection.SelectionPath.Bounds;
        RectI intBounds = (RectI)bounds.Intersect(SKRect.Create(0, 0, target.Size.X, target.Size.Y)).RoundOutwards();

        image.SetClippingPath(target.Selection.SelectionPath);
        image.EnqueueClearRegion(intBounds);
        var affChunks = image.FindAffectedChunks();
        savedChunks = new(image, affChunks);
        image.CommitChanges();
        ignoreInUndo = false;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affChunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affectedChunks = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, ref savedChunks);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affectedChunks, drawOnMask);
    }

    public override void Dispose()
    {
        savedChunks?.Dispose();
    }
}
