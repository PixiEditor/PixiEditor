using ChunkyImageLib.Operations;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
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
    private float strokeWidth;
    private readonly bool erasing;
    private readonly bool drawOnMask;
    private readonly bool antiAliasing;
    private bool squareBrush;
    private float hardness;
    private float spacing = 1;
    private readonly Paint srcPaint = new Paint() { BlendMode = BlendMode.Src };
    private Paintable? finalPaintable;

    private CommittedChunkStorage? storedChunks;
    private readonly List<VecI> points = new();
    private int frame;
    private VecF lastPos;
    private int lastAppliedPointIndex = -1;

    [GenerateUpdateableChangeActions]
    public LineBasedPen_UpdateableChange(Guid memberGuid, Color color, VecI pos, float strokeWidth, bool erasing,
        bool antiAliasing,
        float hardness,
        float spacing,
        bool squareBrush,
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
        this.squareBrush = squareBrush;
        points.Add(pos);
        this.frame = frame;

        srcPaint.Shader?.Dispose();
        srcPaint.Shader = null;

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
    public void Update(VecI pos, float strokeWidth)
    {
        if (points.Count > 0)
        {
            var bresenham = BresenhamLineHelper.GetBresenhamLine(points[^1], pos);
            points.AddRange(bresenham);
        }

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

        int opCount = image.QueueLength;

        float spacingPixels = strokeWidth * spacing;

        for (int i = Math.Max(lastAppliedPointIndex, 0); i < points.Count; i++)
        {
            var point = points[i];
            if (points.Count > 1 && VecF.Distance(lastPos, point) < spacingPixels)
                continue;

            lastPos = point;
            var rect = new RectI(point - new VecI((int)(strokeWidth / 2f)), new VecI((int)strokeWidth));
            finalPaintable = color;

            if (!squareBrush)
            {
                if (antiAliasing)
                {
                    finalPaintable = ApplySoftnessGradient((VecD)point);
                }

                image.EnqueueDrawEllipse((RectD)rect, finalPaintable, finalPaintable, 0, 0, antiAliasing, srcPaint);
            }
            else
            {
                BlendMode blendMode = srcPaint.BlendMode;
                ShapeData shapeData = new ShapeData(rect.Center, rect.Size, 0, 0, 0, finalPaintable, finalPaintable,
                    blendMode);
                image.EnqueueDrawRectangle(shapeData);
            }
        }

        lastAppliedPointIndex = points.Count - 1;

        var affChunks = image.FindAffectedArea(opCount);

        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affChunks, drawOnMask);
    }

    private void FastforwardEnqueueDrawLines(ChunkyImage targetImage)
    {
        if (points.Count == 1)
        {
            var rect = new RectI(points[0] - new VecI((int)(strokeWidth / 2f)), new VecI((int)strokeWidth));
            finalPaintable = color;

            if (!squareBrush)
            {
                if (antiAliasing)
                {
                    finalPaintable = ApplySoftnessGradient(points[0]);
                }

                targetImage.EnqueueDrawEllipse((RectD)rect, finalPaintable, finalPaintable, 0, 0, antiAliasing,
                    srcPaint);
            }
            else
            {
                BlendMode blendMode = srcPaint.BlendMode;
                ShapeData shapeData = new ShapeData(rect.Center, rect.Size, 0, 0, 0, finalPaintable, finalPaintable,
                    blendMode);
                targetImage.EnqueueDrawRectangle(shapeData);
            }

            return;
        }

        VecF lastPos = points[0];

        float spacingInPixels = strokeWidth * this.spacing;

        for (int i = 0; i < points.Count; i++)
        {
            if (i > 0 && VecF.Distance(lastPos, points[i]) < spacingInPixels)
                continue;

            lastPos = points[i];
            var rect = new RectI(points[i] - new VecI((int)(strokeWidth / 2f)), new VecI((int)strokeWidth));
            finalPaintable = color;

            if (!squareBrush)
            {
                if (antiAliasing)
                {
                    finalPaintable = ApplySoftnessGradient(points[i]);
                }

                targetImage.EnqueueDrawEllipse((RectD)rect, finalPaintable, finalPaintable, 0, 0, antiAliasing,
                    srcPaint);
            }
            else
            {
                BlendMode blendMode = srcPaint.BlendMode;
                ShapeData shapeData = new ShapeData(rect.Center, rect.Size, 0, 0, 0, finalPaintable, finalPaintable,
                    blendMode);
                targetImage.EnqueueDrawRectangle(shapeData);
            }
        }
    }

    private Paintable? ApplySoftnessGradient(VecD pos)
    {
        srcPaint.Paintable?.Dispose();
        if (hardness >= 1)
        {
            return new ColorPaintable(color);
        }

        float radius = strokeWidth / 2f;
        radius = MathF.Max(1, radius);
        return new RadialGradientPaintable(pos, radius,
        [
            new GradientStop(color, Math.Max(hardness - 0.05f, 0)),
            new GradientStop(color.WithAlpha(0), 0.95f)
        ]) { AbsoluteValues = true };
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
        srcPaint.Dispose();
    }
}
