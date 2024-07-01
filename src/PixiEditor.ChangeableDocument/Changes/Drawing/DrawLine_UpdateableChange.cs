using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class DrawLine_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private VecI from;
    private VecI to;
    private int strokeWidth;
    private Color color;
    private StrokeCap caps;
    private readonly bool drawOnMask;
    private CommittedChunkStorage? savedChunks;
    private int frame;

    [GenerateUpdateableChangeActions]
    public DrawLine_UpdateableChange
        (Guid memberGuid, VecI from, VecI to, int strokeWidth, Color color, StrokeCap caps, bool drawOnMask, int frame)
    {
        this.memberGuid = memberGuid;
        this.from = from;
        this.to = to;
        this.strokeWidth = strokeWidth;
        this.color = color;
        this.caps = caps;
        this.drawOnMask = drawOnMask;
        this.frame = frame;
    }

    [UpdateChangeMethod]
    public void Update(VecI from, VecI to, int strokeWidth, Color color, StrokeCap caps)
    {
        this.from = from;
        this.to = to;
        this.color = color;
        this.caps = caps;
        this.strokeWidth = strokeWidth;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask);
    }

    private AffectedArea CommonApply(Document target)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);
        var oldAffected = image.FindAffectedArea();
        image.CancelChanges();
        if (from != to)
        {
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);
            if (strokeWidth == 1)
                image.EnqueueDrawBresenhamLine(from, to, color, BlendMode.SrcOver);
            else
                image.EnqueueDrawSkiaLine(from, to, caps, strokeWidth, color, BlendMode.SrcOver);
        }
        var totalAffected = image.FindAffectedArea();
        totalAffected.UnionWith(oldAffected);
        return totalAffected;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, CommonApply(target), drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (from == to)
        {
            ignoreInUndo = true;
            return new None();
        }

        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);
        var affected = CommonApply(target);
        if (savedChunks is not null)
            throw new InvalidOperationException("Trying to save chunks while there are saved chunks already");
        savedChunks = new CommittedChunkStorage(image, image.FindAffectedArea().Chunks);
        image.CommitChanges();

        ignoreInUndo = false;
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affected, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull
            (target, memberGuid, drawOnMask, frame, ref savedChunks);
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affected, drawOnMask);
    }

    public override void Dispose()
    {
        savedChunks?.Dispose();
    }
}
