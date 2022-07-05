using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class DrawLine_UpdateableChange : UpdateableChange 
{
    private readonly Guid memberGuid;
    private VecI from;
    private VecI to;
    private int strokeWidth;
    private SKColor color;
    private SKStrokeCap caps;
    private readonly bool drawOnMask;
    private CommittedChunkStorage? savedChunks;

    [GenerateUpdateableChangeActions]
    public DrawLine_UpdateableChange
        (Guid memberGuid, VecI from, VecI to, int strokeWidth, SKColor color, SKStrokeCap caps, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.from = from;
        this.to = to;
        this.strokeWidth = strokeWidth;
        this.color = color;
        this.caps = caps;
        this.drawOnMask = drawOnMask;
    }

    [UpdateChangeMethod]
    public void Update(VecI from, VecI to, int strokeWidth, SKColor color, SKStrokeCap caps)
    {
        this.from = from;
        this.to = to;
        this.color = color;
        this.caps = caps;
        this.strokeWidth = strokeWidth;
    }
    
    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return new Error();
        return new Success();
    }

    private HashSet<VecI> CommonApply(Document target)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        var oldAffected = image.FindAffectedChunks();
        image.CancelChanges();
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);
        if (strokeWidth == 1)
            image.EnqueueDrawBresenhamLine(from, to, color, SKBlendMode.SrcOver);
        else
            image.EnqueueDrawSkiaLine(from, to, caps, strokeWidth, color, SKBlendMode.SrcOver);
        var totalAffected = image.FindAffectedChunks();
        totalAffected.UnionWith(oldAffected);
        return totalAffected;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, CommonApply(target), drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        var affected = CommonApply(target);
        if (savedChunks is not null)
            throw new InvalidOperationException("Trying to save chunks while there are saved chunks already");
        savedChunks = new CommittedChunkStorage(image, image.FindAffectedChunks());
        image.CommitChanges();
        
        ignoreInUndo = false;
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affected, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull
            (target, memberGuid, drawOnMask, ref savedChunks);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affected, drawOnMask);
    }

    public override void Dispose()
    {
        savedChunks?.Dispose();
    }
}
