using ChunkyImageLib.Operations;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class LineBasedPen_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private readonly Color color;
    private int strokeWidth;
    private readonly bool erasing;
    private readonly bool drawOnMask;
    private readonly bool antiAliasing;
    private float hardness;
    private float spacing = 1;
    private readonly Paint srcPaint = new Paint() { BlendMode = BlendMode.Src };

    private CommittedChunkStorage? storedChunks;
    private readonly List<VecI> points = new();
    private int frame;
    private VecF lastPos;

    [GenerateUpdateableChangeActions]
    public LineBasedPen_UpdateableChange(Guid memberGuid, Color color, VecI pos, int strokeWidth, bool erasing,
        bool antiAliasing,
        float hardness,
        float spacing,
        bool drawOnMask, int frame)
    {
        this.memberGuid = memberGuid;
        this.color = color;
        this.strokeWidth = strokeWidth;
        this.erasing = erasing;
        this.antiAliasing = antiAliasing;
        this.drawOnMask = drawOnMask;
        this.hardness = hardness;
        this.spacing = spacing;
        points.Add(pos);
        this.frame = frame;
        if (this.antiAliasing && !erasing)
        {
            srcPaint.BlendMode = BlendMode.SrcOver;
        }
        else if (erasing)
        {
            srcPaint.BlendMode = BlendMode.DstOut;
        }
    }

    [UpdateChangeMethod]
    public void Update(VecI pos, int strokeWidth)
    {
        points.Add(pos);
        this.strokeWidth = strokeWidth;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return false;
        if (strokeWidth < 1)
            return false;
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);
        if (!erasing)
            image.SetBlendMode(BlendMode.SrcOver);
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);
        srcPaint.IsAntiAliased = antiAliasing;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);

        var (from, to) = points.Count > 1 ? (points[^2], points[^1]) : (points[0], points[0]);

        int opCount = image.QueueLength;

        var bresenham = BresenhamLineHelper.GetBresenhamLine(from, to);
        
        float spacingPixels = strokeWidth * spacing;

        foreach (var point in bresenham)
        {
            if (points.Count > 1 && VecF.Distance(lastPos, point) < spacingPixels)
                continue;

            lastPos = point;
            var rect = new RectI(point - new VecI(strokeWidth / 2), new VecI(strokeWidth));
            if (antiAliasing)
            {
                ApplySoftnessGradient((VecD)point);
            }
            
            image.EnqueueDrawEllipse(rect, color, color, 0, 0, antiAliasing, srcPaint);
        }

        var affChunks = image.FindAffectedArea(opCount);

        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affChunks, drawOnMask);
    }

    private void FastforwardEnqueueDrawLines(ChunkyImage targetImage)
    {
        if (points.Count == 1)
        {
            var rect = new RectI(points[0] - new VecI(strokeWidth / 2), new VecI(strokeWidth));
            targetImage.EnqueueDrawEllipse(rect, color, color, 1, 0, antiAliasing, srcPaint);
            return;
        }

        VecF lastPos = points[0];
        
        float spacingInPixels = strokeWidth * this.spacing;

        for (int i = 0; i < points.Count; i++)
        {
            if (i > 0 && VecF.Distance(lastPos, points[i]) < spacingInPixels)
                continue;

            lastPos = points[i];
            var rect = new RectI(points[i] - new VecI(strokeWidth / 2), new VecI(strokeWidth));
            if (antiAliasing)
            {
                ApplySoftnessGradient(points[i]);
            }

            targetImage.EnqueueDrawEllipse(rect, color, color, 0, 0, antiAliasing, srcPaint);
        }
    }

    private void ApplySoftnessGradient(VecD pos)
    {
        srcPaint.Shader?.Dispose();
        float radius = strokeWidth / 2f;
        radius = MathF.Max(1, radius);
        srcPaint.Shader = Shader.CreateRadialGradient(
            pos, radius, [color, color.WithAlpha(0)],
            [hardness - 0.03f, 1f], ShaderTileMode.Clamp);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        if (storedChunks is not null)
            throw new InvalidOperationException("Trying to save chunks while there are saved chunks already");
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);

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
            if (!erasing)
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
        var affected =
            DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, frame,
                ref storedChunks);
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affected, drawOnMask);
    }

    public override void Dispose()
    {
        storedChunks?.Dispose();
    }
}
