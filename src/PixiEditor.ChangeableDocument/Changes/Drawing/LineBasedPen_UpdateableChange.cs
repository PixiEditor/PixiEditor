using System.Reflection;
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
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.ChangeableDocument.Rendering.ContextData;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class LineBasedPen_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private readonly Color color;
    private float strokeWidth;
    private readonly bool erasing;
    private readonly bool drawOnMask;
    private readonly bool antiAliasing;
    private BrushData brushData;
    private BrushEngine engine = new BrushEngine();
    private float spacing = 1;
    private readonly Paint srcPaint = new Paint() { BlendMode = BlendMode.Src };

    private CommittedChunkStorage? storedChunks;
    private readonly List<VecI> points = new();
    private int frame;
    private BrushOutputNode? brushOutputNode;
    private PointerInfo pointerInfo;
    private EditorData editorData;

    [GenerateUpdateableChangeActions]
    public LineBasedPen_UpdateableChange(Guid memberGuid, Color color, VecI pos, float strokeWidth, bool erasing,
        bool antiAliasing,
        float spacing,
        BrushData brushData,
        bool drawOnMask, int frame, PointerInfo pointerInfo, EditorData editorData)
    {
        this.memberGuid = memberGuid;
        this.color = color;
        this.strokeWidth = strokeWidth;
        this.erasing = erasing;
        this.antiAliasing = antiAliasing;
        this.drawOnMask = drawOnMask;
        this.spacing = spacing;
        this.brushData = brushData;
        points.Add(pos);
        this.frame = frame;
        this.pointerInfo = pointerInfo;
        this.editorData = editorData;

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
    public void Update(VecI pos, float strokeWidth, float spacing, PointerInfo pointerInfo)
    {
        if (points.Count > 0)
        {
            var bresenham = BresenhamLineHelper.GetBresenhamLine(points[^1], pos);
            points.AddRange(bresenham);
        }

        this.strokeWidth = strokeWidth;
        this.pointerInfo = pointerInfo;
        this.spacing = spacing;
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

        brushOutputNode = brushData.BrushGraph?.AllNodes.FirstOrDefault(x => x is BrushOutputNode) as BrushOutputNode;
        UpdateBrushData();

        return brushOutputNode != null;
    }

    private void UpdateBrushData()
    {
        if (brushOutputNode != null)
        {
            brushData = new BrushData(brushData.BrushGraph)
            {
                StrokeWidth = strokeWidth, AntiAliasing = antiAliasing, Spacing = spacing,
            };
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);

        int opCount = image.QueueLength;

        brushData.AntiAliasing = antiAliasing;
        brushData.Spacing = spacing;
        brushData.StrokeWidth = strokeWidth;

        engine.ExecuteBrush(image, brushData, points, frame, target.ProcessingColorSpace, SamplingOptions.Default,
            pointerInfo, editorData);

        var affChunks = image.FindAffectedArea(opCount);

        var changeInfo = DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affChunks, drawOnMask);
        List<IChangeInfo> changes = new()
        {
            changeInfo.AsT1,
            /*new ComputedPropertyValue_ChangeInfo(brushOutputGuid, "VectorShape", true,
                brushOutputNode.VectorShape.Value)*/
        };

        return changes;
    }

    private void FastforwardEnqueueDrawLines(ChunkyImage targetImage, KeyFrameTime frameTime)
    {
        brushData.AntiAliasing = antiAliasing;
        brushData.Spacing = spacing;
        brushData.StrokeWidth = strokeWidth;

        if (points.Count == 1)
        {
            engine.ExecuteBrush(targetImage, brushData, points[0], frameTime, targetImage.ProcessingColorSpace,
                SamplingOptions.Default, pointerInfo, editorData);

            return;
        }

        engine.ExecuteBrush(targetImage, brushData, points, frameTime, targetImage.ProcessingColorSpace,
            SamplingOptions.Default, pointerInfo, editorData);
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

            var change = DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask).AsT1;
            List<IChangeInfo> changes = new()
            {
                change,
                /*new ComputedPropertyValue_ChangeInfo(brushOutputGuid, "VectorShape", true,
                    brushOutputNode?.VectorShape.Value)*/
            };
            return changes;
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

            IChangeInfo info = DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask).AsT1;

            List<IChangeInfo> changes = new()
            {
                info,
                /*new ComputedPropertyValue_ChangeInfo(brushOutputGuid, "VectorShape", true,
                    brushOutputNode?.VectorShape.Value)*/
            };

            return changes;
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
