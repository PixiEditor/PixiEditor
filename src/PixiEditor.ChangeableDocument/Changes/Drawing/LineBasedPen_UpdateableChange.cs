using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class LineBasedPen_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private readonly Color color;
    private readonly int strokeWidth;
    private readonly bool replacing;
    private readonly bool drawOnMask;
    private readonly Paint srcPaint = new Paint() { BlendMode = BlendMode.Src };

    private CommittedChunkStorage? storedChunks;
    private readonly List<VecI> points = new();

    [GenerateUpdateableChangeActions]
    public LineBasedPen_UpdateableChange(Guid memberGuid, Color color, VecI pos, int strokeWidth, bool replacing, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.color = color;
        this.strokeWidth = strokeWidth;
        this.replacing = replacing;
        this.drawOnMask = drawOnMask;
        points.Add(pos);
    }

    [UpdateChangeMethod]
    public void Update(VecI pos)
    {
        points.Add(pos);
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return false;
        if (strokeWidth < 1)
            return false;
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        if (!replacing)
            image.SetBlendMode(BlendMode.SrcOver);
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);

        var (from, to) = points.Count > 1 ? (points[^2], points[^1]) : (points[0], points[0]);

        int opCount = image.QueueLength;

        if (strokeWidth == 1)
        {
            image.EnqueueDrawBresenhamLine(from, to, color, BlendMode.Src);
        }
        else
        {
            var rect = new RectI(to - new VecI(strokeWidth / 2), new VecI(strokeWidth));
            image.EnqueueDrawEllipse(rect, color, color, 1, srcPaint);
            image.EnqueueDrawSkiaLine(from, to, StrokeCap.Butt, strokeWidth, color, BlendMode.Src);
        }
        var affChunks = image.FindAffectedArea(opCount);

        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affChunks, drawOnMask);
    }

    private void FastforwardEnqueueDrawLines(ChunkyImage targetImage)
    {
        if (points.Count == 1)
        {
            if (strokeWidth == 1)
            {
                targetImage.EnqueueDrawBresenhamLine(points[0], points[0], color, BlendMode.Src);
            }
            else
            {
                var rect = new RectI(points[0] - new VecI(strokeWidth / 2), new VecI(strokeWidth));
                targetImage.EnqueueDrawEllipse(rect, color, color, 1, srcPaint);
            }
            return;
        }

        var firstRect = new RectI(points[0] - new VecI(strokeWidth / 2), new VecI(strokeWidth));
        targetImage.EnqueueDrawEllipse(firstRect, color, color, 1, srcPaint);

        for (int i = 1; i < points.Count; i++)
        {
            if (strokeWidth == 1)
            {
                targetImage.EnqueueDrawBresenhamLine(points[i - 1], points[i], color, BlendMode.Src);
            }
            else
            {
                var rect = new RectI(points[i] - new VecI(strokeWidth / 2), new VecI(strokeWidth));
                targetImage.EnqueueDrawEllipse(rect, color, color, 1, srcPaint);
                targetImage.EnqueueDrawSkiaLine(points[i - 1], points[i], StrokeCap.Butt, strokeWidth, color, BlendMode.Src);
            }
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (storedChunks is not null)
            throw new InvalidOperationException("Trying to save chunks while there are saved chunks already");
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);

        ignoreInUndo = false;
        if (firstApply)
        {
            var affArea = image.FindAffectedArea();
            storedChunks = new CommittedChunkStorage(image, affArea.Chunks);
            image.CommitChanges();

            return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask);
        }
        else
        {
            if (!replacing)
                image.SetBlendMode(BlendMode.SrcOver);
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);

            FastforwardEnqueueDrawLines(image);
            var affArea = image.FindAffectedArea();
            storedChunks = new CommittedChunkStorage(image, affArea.Chunks);
            image.CommitChanges();

            return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask);
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, ref storedChunks);
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affected, drawOnMask);
    }

    public override void Dispose()
    {
        storedChunks?.Dispose();
    }
}
