using Drawie.Backend.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class DrawRasterRectangle_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private ShapeData rect;
    private readonly bool drawOnMask;
    private CommittedChunkStorage? storedChunks;
    private int frame;
    
    [GenerateUpdateableChangeActions]
    public DrawRasterRectangle_UpdateableChange(Guid memberGuid, ShapeData rectangle, bool drawOnMask, int frame)
    {
        this.memberGuid = memberGuid;
        this.rect = rectangle;
        this.drawOnMask = drawOnMask;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask, frame);
    }

    [UpdateChangeMethod]
    public void Update(ShapeData rectangle)
    {
        rect = rectangle;
    }

    private AffectedArea UpdateRectangle(Document target, ChunkyImage targetImage)
    {
        var oldAffArea = targetImage.FindAffectedArea();

        targetImage.CancelChanges();

        if (!(rect.Size.X <= 0 || rect.Size.Y <= 0))
        {
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, targetImage, memberGuid, drawOnMask);
            targetImage.EnqueueDrawRectangle(rect);
        }

        var affArea = targetImage.FindAffectedArea();
        affArea.UnionWith(oldAffArea);

        return affArea;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);
        var area = UpdateRectangle(target, targetImage);
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, area, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (rect.Size.X <= 0 || rect.Size.Y <= 0)
        {
            ignoreInUndo = true;
            return new None();
        }

        ChunkyImage targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);
        var area = UpdateRectangle(target, targetImage);
        storedChunks = new CommittedChunkStorage(targetImage, area.Chunks);
        targetImage.CommitChanges();

        ignoreInUndo = false;
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, area, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var area = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, frame, ref storedChunks);
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, area, drawOnMask);
    }

    public override void Dispose()
    {
        storedChunks?.Dispose();
    }
}
