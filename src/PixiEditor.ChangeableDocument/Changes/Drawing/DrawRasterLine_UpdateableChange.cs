using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class DrawRasterLine_UpdateableChange : InterruptableUpdateableChange
{
    private readonly Guid memberGuid;
    private VecD from;
    private VecD to;
    private float strokeWidth;
    private Paintable paintable;
    private StrokeCap caps;
    private readonly bool drawOnMask;
    private CommittedChunkStorage? savedChunks;
    private int frame;
    private bool antiAliasing;
    private Paint paint;

    [GenerateUpdateableChangeActions]
    public DrawRasterLine_UpdateableChange
    (Guid memberGuid, VecD from, VecD to, float strokeWidth, Paintable paintable, StrokeCap caps, bool antiAliasing,
        bool drawOnMask, int frame)
    {
        this.memberGuid = memberGuid;
        this.from = from;
        this.to = to;
        this.strokeWidth = strokeWidth;
        this.paintable = paintable;
        this.caps = caps;
        this.drawOnMask = drawOnMask;
        this.frame = frame;
        this.antiAliasing = antiAliasing;

        paint = new Paint()
        {
            StrokeWidth = strokeWidth, StrokeCap = caps, IsAntiAliased = antiAliasing, BlendMode = BlendMode.SrcOver
        };
        paint.SetPaintable(paintable);
    }

    [UpdateChangeMethod]
    public void Update(VecD from, VecD to, float strokeWidth, Paintable paintable, StrokeCap caps)
    {
        this.from = from;
        this.to = to;
        this.paintable = paintable;
        this.caps = caps;
        this.strokeWidth = strokeWidth;

        paint.SetPaintable(paintable);
        paint.StrokeWidth = strokeWidth;
        paint.StrokeCap = caps;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask, frame);
    }

    private AffectedArea CommonApply(Document target)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);
        var oldAffected = image.FindAffectedArea();
        image.CancelChanges();
        if (from != to)
        {
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);
            if (strokeWidth == 0)
            {
                image.CancelChanges();
            }
            else
            {
                if (Math.Abs(strokeWidth - 1) < 0.01f && !antiAliasing)
                {
                    image.EnqueueDrawBresenhamLine((VecI)from, (VecI)to, paintable, BlendMode.SrcOver);
                }
                else
                {
                    image.EnqueueDrawSkiaLine(from, to, paint);
                }
            }
        }

        var totalAffected = image.FindAffectedArea();
        totalAffected.UnionWith(oldAffected);
        return totalAffected;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, CommonApply(target), drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
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
        paint?.Dispose();
    }
}
