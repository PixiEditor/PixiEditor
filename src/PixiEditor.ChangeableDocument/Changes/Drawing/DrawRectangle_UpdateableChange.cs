namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class DrawRectangle_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private ShapeData rect;
    private readonly bool drawOnMask;
    private CommittedChunkStorage? storedChunks;

    [GenerateUpdateableChangeActions]
    public DrawRectangle_UpdateableChange(Guid memberGuid, ShapeData rectangle, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.rect = rectangle;
        this.drawOnMask = drawOnMask;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return new Error();
        return new Success();
    }

    [UpdateChangeMethod]
    public void Update(ShapeData rectangle)
    {
        rect = rectangle;
    }

    private HashSet<VecI> UpdateRectangle(Document target, ChunkyImage targetImage)
    {
        var oldAffectedChunks = targetImage.FindAffectedChunks();

        targetImage.CancelChanges();
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, targetImage, memberGuid, drawOnMask);
        targetImage.EnqueueDrawRectangle(rect);

        var affectedChunks = targetImage.FindAffectedChunks();
        affectedChunks.UnionWith(oldAffectedChunks);

        return affectedChunks;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        var chunks = UpdateRectangle(target, targetImage);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, chunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        var affectedChunks = UpdateRectangle(target, targetImage);
        storedChunks = new CommittedChunkStorage(targetImage, affectedChunks);
        targetImage.CommitChanges();

        ignoreInUndo = false;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affectedChunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affectedChunks = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, ref storedChunks);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affectedChunks, drawOnMask);
    }

    public override void Dispose()
    {
        storedChunks?.Dispose();
    }
}
