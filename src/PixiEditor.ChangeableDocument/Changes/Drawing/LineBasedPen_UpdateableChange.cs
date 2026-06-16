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
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
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

    [GenerateUpdateableChangeActions]
    public LineBasedPen_UpdateableChange(Guid memberGuid, VecD pos, float strokeWidth,
        bool antiAliasing,
        BrushData brushData,
        bool drawOnMask, int frame, PointerInfo pointerInfo, KeyboardInfo keyboardInfo, EditorData editorData)
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
    }

    [UpdateChangeMethod]
    public void Update(VecD pos, float strokeWidth, PointerInfo pointerInfo, KeyboardInfo keyboardInfo,
        EditorData editorData, BrushData brushData)
    {
        points.Add(new RecordedPoint(pos, pointerInfo, keyboardInfo, editorData));
        this.strokeWidth = strokeWidth;
        this.pointerInfo = pointerInfo;
        this.keyboardInfo = keyboardInfo;
        this.editorData = editorData;
        this.brushData = brushData;
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
        engine.ExecuteBrush(image, brushData, points, frame, target.ProcessingColorSpace, SamplingOptions.Default);

        var affChunks = image.FindAffectedArea(opCount);

        var changeInfo = DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affChunks, drawOnMask);

        return changeInfo;
    }

    private void FastforwardEnqueueDrawLines(ChunkyImage targetImage, KeyFrameTime frameTime)
    {
        brushData.AntiAliasing = antiAliasing;
        brushData.StrokeWidth = strokeWidth;
        engine.ResetState();

        if (points.Count == 1)
        {
            engine.ExecuteBrush(targetImage, brushData, points[0].Position, frameTime, targetImage.ProcessingColorSpace,
                SamplingOptions.Default, pointerInfo, keyboardInfo, editorData);

            return;
        }

        engine.ExecuteBrush(targetImage, brushData, points, frameTime, targetImage.ProcessingColorSpace,
            SamplingOptions.Default);
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

            FastforwardEnqueueDrawLines(image, frame);
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
