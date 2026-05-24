using System.Diagnostics;
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
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.ChangeableDocument.Rendering.ContextData;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class LineBasedPen_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private float strokeWidth;
    private readonly bool drawOnMask;
    private readonly bool antiAliasing;
    private BrushData brushData;
    private BrushEngine engine = new BrushEngine();

    private CommittedChunkStorage? storedChunks;
    private readonly List<RecordedPoint> points = new();

    private int cachedCount = -1;
    private int frame;
    private BrushOutputNode? brushOutputNode;
    private PointerInfo pointerInfo;
    private KeyboardInfo keyboardInfo;
    private EditorData editorData;
    private ViewportData viewportData;
    private string? customOutput;

    [GenerateUpdateableChangeActions]
    public LineBasedPen_UpdateableChange(Guid memberGuid, VecD pos, float strokeWidth,
        bool antiAliasing,
        BrushData brushData,
        bool drawOnMask, int frame, PointerInfo pointerInfo, KeyboardInfo keyboardInfo, EditorData editorData, ViewportData viewportData, string? activeCustomOutput)
    {
        this.memberGuid = memberGuid;
        this.strokeWidth = strokeWidth;
        this.antiAliasing = antiAliasing;
        this.drawOnMask = drawOnMask;
        this.brushData = brushData;
        points.Add(new RecordedPoint(pos, pointerInfo, keyboardInfo, editorData));
        this.frame = frame;
        this.pointerInfo = pointerInfo;
        this.keyboardInfo = keyboardInfo;
        this.editorData = editorData;
        this.customOutput = activeCustomOutput;
        this.viewportData = viewportData;
    }

    [UpdateChangeMethod]
    public void Update(VecD pos, float strokeWidth, PointerInfo pointerInfo, KeyboardInfo keyboardInfo,
        EditorData editorData, BrushData brushData, ViewportData viewportData)
    {
        points.Add(new RecordedPoint(pos, pointerInfo, keyboardInfo, editorData));
        this.strokeWidth = strokeWidth;
        this.pointerInfo = pointerInfo;
        this.keyboardInfo = keyboardInfo;
        this.editorData = editorData;
        this.brushData = brushData;
        this.viewportData = viewportData;
        UpdateBrushData();
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask, frame))
            return false;
        if (strokeWidth < 0.1)
            return false;
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);

        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);

        brushOutputNode = brushData.BrushGraph?.AllNodes.FirstOrDefault(x => x is BrushOutputNode) as BrushOutputNode;
        UpdateBrushData();

        return brushOutputNode != null;
    }

    private void UpdateBrushData()
    {
        if (brushOutputNode != null)
        {
            brushData = new BrushData(brushData.BrushGraph, brushData.TargetBrushNodeId)
            {
                StrokeWidth = strokeWidth, AntiAliasing = antiAliasing
            };
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);

        int opCount = image.QueueLength;

        brushData.AntiAliasing = antiAliasing;
        brushData.StrokeWidth = strokeWidth;

        // TODO: Sampling options?
        Matrix3X3 inputTransformer = TryGetInputTransformer(target, customOutput);
        engine.ExecuteBrush(image, brushData, points, frame, target.ProcessingColorSpace, SamplingOptions.Default, viewportData, inputTransformer);

        var affChunks = image.FindAffectedArea(opCount);

        var changeInfo = DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affChunks, drawOnMask);

        return changeInfo;
    }

    private Matrix3X3 TryGetInputTransformer(Document target, string? output)
    {
        if (output != null)
        {
            if (target.NodeGraph == null)
                return Matrix3X3.Identity;

            if (!string.Equals(output, "default", StringComparison.OrdinalIgnoreCase))
            {
                if (target.NodeGraph.Nodes.FirstOrDefault(n =>
                        n is CustomOutputNode cout && cout.OutputName.Value == customOutput &&
                        cout.InputTransform.Connection != null) is CustomOutputNode matchingTransfomer)
                {
                    target.NodeGraph.Execute(matchingTransfomer, new BrushRenderContext(
                    null,
                    frame,
                    ChunkResolution.Full,
                    target.Size, target.Size,
                    target.ProcessingColorSpace,
                    SamplingOptions.Default, brushData, null, VecD.Zero, null, target.NodeGraph,
                    VecD.Zero, VecD.Zero)
                {
                    PointerInfo = pointerInfo,
                    KeyboardInfo = keyboardInfo,
                    EditorData = editorData,
                    ViewportData = viewportData
                });

                    return matchingTransfomer.InputTransform.Value;
                }
            }
            else
            {
                target.NodeGraph.Execute(new BrushRenderContext(
                    null,
                    frame,
                    ChunkResolution.Full,
                    target.Size, target.Size,
                    target.ProcessingColorSpace,
                    SamplingOptions.Default, brushData, null, VecD.Zero, null, target.NodeGraph,
                    VecD.Zero, VecD.Zero)
                {
                    PointerInfo = pointerInfo,
                    KeyboardInfo = keyboardInfo,
                    EditorData = editorData,
                    ViewportData = viewportData
                });

                return (target.NodeGraph.OutputNode as OutputNode)?.InputTransform?.Value ?? Matrix3X3.Identity;
            }
        }

        return Matrix3X3.Identity;
    }

    private void FastforwardEnqueueDrawLines(ChunkyImage targetImage, KeyFrameTime frameTime,
        Matrix3X3 inputTransformer)
    {
        brushData.AntiAliasing = antiAliasing;
        brushData.StrokeWidth = strokeWidth;
        engine.ResetState();

        if (points.Count == 1)
        {
            var point = inputTransformer.MapPoint(points[0].Position);
            engine.ExecuteBrush(targetImage, brushData, point, frameTime, targetImage.ProcessingColorSpace,
                SamplingOptions.Default, pointerInfo, keyboardInfo, editorData, viewportData);

            return;
        }

        engine.ExecuteBrush(targetImage, brushData, points, frameTime, targetImage.ProcessingColorSpace,
            SamplingOptions.Default, viewportData, inputTransformer);
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
            return OneOf<None, IChangeInfo, List<IChangeInfo>>.FromT1(change);
        }
        else
        {
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);

            Matrix3X3 inputTransformer = TryGetInputTransformer(target, customOutput);
            FastforwardEnqueueDrawLines(image, frame, inputTransformer);
            var affArea = image.FindAffectedArea();
            storedChunks = new CommittedChunkStorage(image, affArea.Chunks);
            image.CommitChanges();

            IChangeInfo info = DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask).AsT1;

            return OneOf<None, IChangeInfo, List<IChangeInfo>>.FromT1(info);
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
        engine?.Dispose();
    }
}
