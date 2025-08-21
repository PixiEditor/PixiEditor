using ChunkyImageLib.Operations;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class LineBasedPen_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private readonly Color color;
    private float strokeWidth;
    private readonly bool erasing;
    private readonly bool drawOnMask;
    private readonly bool antiAliasing;
    private Guid brushOutputGuid;
    private BrushData brushData;
    private BrushEngine engine = new BrushEngine();
    private float hardness;
    private float spacing = 1;
    private readonly Paint srcPaint = new Paint() { BlendMode = BlendMode.Src };
    private Paintable? finalPaintable;

    private CommittedChunkStorage? storedChunks;
    private readonly List<VecI> points = new();
    private int frame;
    private VecF lastPos;
    private int lastAppliedPointIndex = -1;
    private BrushOutputNode? brushOutputNode;

    [GenerateUpdateableChangeActions]
    public LineBasedPen_UpdateableChange(Guid memberGuid, Color color, VecI pos, float strokeWidth, bool erasing,
        bool antiAliasing,
        float hardness,
        float spacing,
        Guid brushOutputGuid,
        bool drawOnMask, int frame)
    {
        this.memberGuid = memberGuid;
        this.color = color;
        this.strokeWidth = strokeWidth;
        this.erasing = erasing;
        this.antiAliasing = antiAliasing;
        this.drawOnMask = drawOnMask;
        this.brushOutputGuid = brushOutputGuid;
        this.hardness = hardness;
        this.spacing = spacing;
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
            if (this.color.A == 0)
            {
                this.color = color.WithAlpha(255);
            }
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
        UpdateBrushData();
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

        if (brushOutputGuid != Guid.Empty)
        {
            brushOutputNode = target.FindNode<BrushOutputNode>(brushOutputGuid);
            brushData = new BrushData(target.NodeGraph);
            UpdateBrushData();

            return brushOutputNode != null;
        }

        return true;
    }

    private void UpdateBrushData()
    {
        if (brushOutputNode != null)
        {
            brushData = new BrushData(brushData.BrushGraph)
            {
                StrokeWidth = strokeWidth,
                AntiAliasing = antiAliasing,
                Hardness = hardness,
                Spacing = spacing,
            };
        }
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
            finalPaintable = color;

            brushData.AntiAliasing = antiAliasing;
            brushData.Hardness = hardness;
            brushData.Spacing = spacing;
            brushData.StrokeWidth = strokeWidth;

            engine.ExecuteBrush(image, brushData, point, frame);

            /*if (brushData.VectorShape == null)
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
                /*ShapeData shapeData = new ShapeData(rect.Center, rect.Size, 0, 0, 0, finalPaintable, finalPaintable,
                    blendMode);
                image.EnqueueDrawRectangle(shapeData);#1#

                var path = brushData.VectorShape.ToPath(true);
                path.Offset(brushData.VectorShape.TransformedAABB.Pos - brushData.VectorShape.GeometryAABB.Pos);
                path.Offset(rect.Center - path.Bounds.Center);
                /*VecD scale = new VecD(rect.Size.X / (float)path.Bounds.Width, rect.Size.Y / (float)path.Bounds.Height);
                if (scale.IsNaNOrInfinity())
                {
                    scale = VecD.Zero;
                }
                VecD uniformScale = new VecD(Math.Min(scale.X, scale.Y));
                path.Transform(Matrix3X3.CreateScale((float)uniformScale.X, (float)uniformScale.Y, (float)rect.Center.X, (float)rect.Center.Y));#1#
                image.EnqueueDrawPath(path, finalPaintable, 1, StrokeCap.Butt, blendMode, PaintStyle.StrokeAndFill, true);
            }*/
        }

        lastAppliedPointIndex = points.Count - 1;

        var affChunks = image.FindAffectedArea(opCount);

        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affChunks, drawOnMask);
    }

    private void FastforwardEnqueueDrawLines(ChunkyImage targetImage, KeyFrameTime frameTime)
    {
        brushData.AntiAliasing = antiAliasing;
        brushData.Hardness = hardness;
        brushData.Spacing = spacing;
        brushData.StrokeWidth = strokeWidth;

        if (points.Count == 1)
        {
            var rect = new RectI(points[0] - new VecI((int)(strokeWidth / 2f)), new VecI((int)strokeWidth));
            finalPaintable = color;

            engine.ExecuteBrush(targetImage, brushData, points[0], frameTime);

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

            engine.ExecuteBrush(targetImage, brushData, points[i], frameTime);
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

            FastforwardEnqueueDrawLines(image, frame);
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
