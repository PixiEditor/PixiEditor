using System.Diagnostics;
using ChunkyImageLib.Operations;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using DrawingApiBlendMode = Drawie.Backend.Core.Surfaces.BlendMode;

namespace PixiEditor.ChangeableDocument.Changeables.Brushes;

public class BrushEngine : IDisposable
{
    private const int TargetStampCacheId = 0;
    private const int LatestStampCacheId = 1;
    private const int StartingStampCacheId = 2;
    private const int FullTextureCacheId = 3;
    private const int FullTextureLatestCacheId = 4;

    private static int nextRenderId = 0;
    private int stamps = 0;
    private TextureCache cache = new();
    private VecD lastPos;
    private VecD startPos;
    private double lastPressure = 1.0;
    private int lastAppliedPointIndex = -1;
    private int lastAppliedHistoryIndex = -1;
    private VecI lastCachedTexturePaintableSize = VecI.Zero;
    private TexturePaintable? lastCachedTexturePaintable = null;
    private Matrix3X3 lastCachedTransform = Matrix3X3.Identity;
    private readonly List<RecordedPoint> pointsHistory = new();
    private readonly List<VecD> interpolated = new(128);
    private Dictionary<Guid, bool> graphUsesTargetSampleInput = new();
    private Dictionary<Guid, bool> graphUsesLatestSampleInput = new();
    private Dictionary<Guid, bool> graphUsesStartingSampleInput = new();
    private Dictionary<Guid, bool> graphUsesTargetFullInput = new();
    private Dictionary<Guid, bool> graphUsesLatestFullInput = new();
    private Dictionary<Guid, bool> graphUsesStartingFullInput = new();

    Texture? startingSampleTexture = null;
    Texture? startingFullTexture = null;
    private ChunkyImage? accumulationBuffer;

    private bool drawnOnce = false;

    // Configuration: How many previous points to average.
    // Higher = smoother but more "laggy" pressure response.
    // 10 points is roughly 10 pixels of stroke history.
    public int PressureSmoothingWindowSize { get; set; } = 10;

    public BrushEngine()
    {
        nextRenderId += 2;
    }

    public void ResetState()
    {
        lastAppliedPointIndex = -1;
        lastAppliedHistoryIndex = -1;
        lastPos = VecD.Zero;
        lastPressure = 1.0;
        startPos = VecD.Zero;
        drawnOnce = false;
        pointsHistory.Clear();
        accumulationBuffer?.Dispose();
        accumulationBuffer = null;
        stamps = 0;
    }

    /// <summary>
    /// Calculates a smoothed pressure value based on the previous points in history.
    /// This acts as a low-pass filter to remove jitter.
    /// </summary>
    private float GetSmoothedPressure(double targetPressure)
    {
        if (pointsHistory.Count <= 0)
            return (float)targetPressure;

        double sum = 0;
        int count = 0;

        for (int i = pointsHistory.Count - 1; i >= 0 && count < PressureSmoothingWindowSize; i--)
        {
            sum += pointsHistory[i].PointerInfo.Pressure;
            count++;
        }

        double historicalAverage = sum / count;

        // If the new pressure is significantly higher than history,
        // the user is trying to make a bold stroke. Minimize smoothing.
        if (targetPressure > historicalAverage)
        {
            // "Lerp" towards the target.
            // 0.8f means: "Use 80% raw pressure, 20% historical average"
            float attackFactor = 0.8f;
            return (float)(historicalAverage + (targetPressure - historicalAverage) * attackFactor);
        }

        // If pressure is steady or dropping, use full smoothing to hide jitter.
        sum += targetPressure;
        count++;

        return (float)(sum / count);
    }

    public void ExecuteBrush(ChunkyImage target, BrushData brushData, List<RecordedPoint> points,
        KeyFrameTime frameTime,
        ColorSpace cs, SamplingOptions samplingOptions)
    {
        if (brushData.BrushGraph == null)
        {
            return;
        }

        if (brushData.BrushGraph.TryLookupNode(brushData.TargetBrushNodeId) is not BrushOutputNode brushNode)
        {
            return;
        }

        for (int i = lastAppliedPointIndex + 1; i < points.Count; i++)
        {
            RecordedPoint previousPoint = points[i];
            if (i == 0)
            {
                if (pointsHistory.Count > 0)
                {
                    previousPoint = pointsHistory[^1];
                }
            }
            else
            {
                previousPoint = points[i - 1];
            }

            var currentPoint = points[i];
            var dist = VecD.Distance(previousPoint.Position, currentPoint.Position);

            bool interpolatePoints = !brushNode.AlwaysClear.Value;
            if (dist > 0.5 && interpolatePoints)
            {
                LineHelper.GetInterpolatedPointsNonAlloc(previousPoint.Position,
                    currentPoint.Position, interpolated);

                for (int j = 1; j < interpolated.Count; j++)
                {
                    var pt = interpolated[j];

                    double ratio = VecD.Distance(previousPoint.Position, pt) /
                                   VecD.Distance(previousPoint.Position, currentPoint.Position);

                    double linearTargetPressure = previousPoint.PointerInfo.Pressure +
                                                  (currentPoint.PointerInfo.Pressure -
                                                   previousPoint.PointerInfo.Pressure) * ratio;

                    float smoothedPressure = GetSmoothedPressure(linearTargetPressure);

                    pointsHistory.Add(new RecordedPoint(pt,
                        currentPoint.PointerInfo with { Pressure = smoothedPressure },
                        currentPoint.KeyboardInfo,
                        currentPoint.EditorData));
                }
            }
            else
            {
                float smoothedPressure = GetSmoothedPressure(currentPoint.PointerInfo.Pressure);

                pointsHistory.Add(new RecordedPoint(currentPoint.Position,
                    currentPoint.PointerInfo with { Pressure = smoothedPressure },
                    currentPoint.KeyboardInfo,
                    currentPoint.EditorData));
            }
        }

        lastAppliedPointIndex = points.Count - 1;

        float strokeWidth = brushData.StrokeWidth;
        float spacing = brushNode.Spacing.Value / 100f;
        int startingIndex = Math.Max(lastAppliedHistoryIndex, 0);
        float spacingPressure = pointsHistory.Count < startingIndex + 1
            ? (float)lastPressure
            : pointsHistory[startingIndex].PointerInfo.Pressure;

        for (int i = Math.Max(lastAppliedHistoryIndex, 0); i < pointsHistory.Count; i++)
        {
            var point = pointsHistory[i];

            float spacingPixels = (strokeWidth * spacingPressure) * spacing;

            if (VecD.Distance(lastPos, point.Position) < spacingPixels)
                continue;

            ExecuteVectorShapeBrush(target, brushNode, brushData, point.Position, frameTime, cs, samplingOptions,
                point.PointerInfo,
                point.KeyboardInfo,
                point.EditorData, false, false);

            var originalHorizontalSymmetry = target?.HorizontalSymmetry;
            var originalVerticalSymmetry = target?.VerticalSymmetry;

            if (originalVerticalSymmetry != null)
            {
                VecD reflectedPoint =
                    new VecD(2 * originalVerticalSymmetry.Value - point.Position.X, point.Position.Y);
                ExecuteVectorShapeBrush(target, brushNode, brushData, reflectedPoint, frameTime, cs, samplingOptions,
                    point.PointerInfo with { PositionOnCanvas = reflectedPoint }, point.KeyboardInfo, point.EditorData,
                    true, false);
            }

            if (originalHorizontalSymmetry != null)
            {
                VecD reflectedPoint =
                    new VecD(point.Position.X, 2 * originalHorizontalSymmetry.Value - point.Position.Y);

                ExecuteVectorShapeBrush(target, brushNode, brushData, reflectedPoint, frameTime, cs, samplingOptions,
                    point.PointerInfo with { PositionOnCanvas = reflectedPoint }, point.KeyboardInfo, point.EditorData,
                    false, true);
            }

            if (originalVerticalSymmetry != null && originalHorizontalSymmetry != null)
            {
                VecD reflectedPoint = new VecD(2 * originalVerticalSymmetry.Value - point.Position.X,
                    2 * originalHorizontalSymmetry.Value - point.Position.Y);
                ExecuteVectorShapeBrush(target, brushNode, brushData, reflectedPoint, frameTime, cs, samplingOptions,
                    point.PointerInfo with { PositionOnCanvas = reflectedPoint }, point.KeyboardInfo, point.EditorData,
                    true, true);
            }

            spacingPressure = brushNode.Pressure.Value;

            lastPos = point.Position;
        }

        lastPressure = pointsHistory.Count > 0 ? pointsHistory[^1].PointerInfo.Pressure : 1.0;
        lastAppliedHistoryIndex = pointsHistory.Count - 1;

        //accumulationBuffer?.CommitChanges();
        target?.EnqueueClear();
        target?.EnqueueDrawUpToDateChunkyImage(VecI.Zero, accumulationBuffer);
    }


    public void ExecuteBrush(ChunkyImage? target, BrushData brushData, VecD point, KeyFrameTime frameTime,
        ColorSpace cs,
        SamplingOptions samplingOptions, PointerInfo pointerInfo, KeyboardInfo keyboardInfo, EditorData editorData)
    {
        var brushNode = brushData.BrushGraph?.TryLookupNode(brushData.TargetBrushNodeId) as BrushOutputNode;
        if (brushNode == null)
        {
            return;
        }

        ExecuteVectorShapeBrush(target, brushNode, brushData, point, frameTime, cs, samplingOptions, pointerInfo,
            keyboardInfo,
            editorData, false, false);
    }

    private void ExecuteVectorShapeBrush(ChunkyImage? target, BrushOutputNode brushNode, BrushData brushData,
        VecD point,
        KeyFrameTime frameTime,
        ColorSpace colorSpace, SamplingOptions samplingOptions,
        PointerInfo pointerInfo, KeyboardInfo keyboardInfo, EditorData editorData, bool flipX, bool flipY)
    {
        bool shouldErase = editorData.PrimaryColor.A == 0;

        var imageBlendMode = shouldErase ? DrawingApiBlendMode.DstOut : brushNode.ImageBlendMode.Value;

        if (!drawnOnce)
        {
            startPos = point;
            lastPos = point;
            drawnOnce = true;
            stamps = 0;
            ResetStartingTextures();

            if (target != null)
            {
                accumulationBuffer?.Dispose();
                accumulationBuffer = new ChunkyImage(target.CommittedSize);
            }

            /*
            if (accumulationBuffer != null)
            {
                target?.AddRasterClip(accumulationBuffer);
            }
            */

            target?.SetBlendMode(imageBlendMode);
            target?.SetOpacity(brushNode.Opacity.Value);

            brushNode.ResetContentTexture();
        }

        float strokeWidth = brushData.StrokeWidth;
        var startingRect = new RectD(startPos - new VecD((strokeWidth / 2f)), new VecD(strokeWidth));
        var rect = new RectD(point - new VecD((strokeWidth / 2f)), new VecD(strokeWidth));
        if (brushNode.SnapToPixels.Value)
        {
            VecI vecIpoint = (VecI)point;
            rect = (RectD)new RectI(vecIpoint - new VecI((int)(strokeWidth / 2f)), new VecI((int)strokeWidth));

            VecI vecIStartPoint = (VecI)startPos;
            startingRect = (RectD)new RectI(vecIStartPoint - new VecI((int)(strokeWidth / 2f)),
                new VecI((int)strokeWidth));
        }

        bool requiresLatestSampleTexture = GraphUsesConnections(brushData.BrushGraph, brushNode,
            n => n.LatestSampleTexture.Connections, graphUsesLatestSampleInput);
        bool requiresLatestFullTexture = GraphUsesConnections(brushData.BrushGraph, brushNode,
            n => n.LatestFullTexture.Connections, graphUsesLatestFullInput);
        bool requiresStartingSampleTexture = GraphUsesConnections(brushData.BrushGraph, brushNode,
            n => n.StartingSampleTexture.Connections, graphUsesStartingSampleInput);
        bool requiresStartingFullTexture = GraphUsesConnections(brushData.BrushGraph, brushNode,
            n => n.StartingFullTexture.Connections, graphUsesStartingFullInput);
        bool requiresTargetSampleTexture = GraphUsesConnections(brushData.BrushGraph, brushNode,
            n => n.TargetSampleTexture.Connections, graphUsesTargetSampleInput);
        bool requiresTargetFullTexture = GraphUsesConnections(brushData.BrushGraph, brushNode,
            n => n.TargetFullTexture.Connections, graphUsesTargetFullInput);

        Texture? latestSampleUnderRect = null;
        Texture? targetSampleUnderRect = null;
        Texture? latestFullTexture = null;

        if (brushNode.AlwaysClear.Value)
        {
            target?.EnqueueClear();
        }

        if (rect is { Width: > 0, Height: > 0 } && target != null)
        {
            requiresLatestSampleTexture |= requiresTargetSampleTexture && brushNode.AllowSampleStacking.Value;
            RectI targetRect = (RectI)rect.Round().Inflate(brushNode.TargetOversample.Value);
            if (requiresLatestSampleTexture)
            {
                latestSampleUnderRect =
                    UpdateSurfaceUnderRect(LatestStampCacheId, target, targetRect, colorSpace, true);
                if (!brushNode.AllowSampleStacking.Value && requiresTargetSampleTexture)
                {
                    targetSampleUnderRect =
                        UpdateSurfaceUnderRect(TargetStampCacheId, target, targetRect, colorSpace, false);
                }
                else
                {
                    targetSampleUnderRect = latestSampleUnderRect;
                }
            }
            else if (requiresTargetSampleTexture)
            {
                targetSampleUnderRect =
                    UpdateSurfaceUnderRect(TargetStampCacheId, target, targetRect, colorSpace, false);
            }
        }

        if (target != null && startingRect is { Width: > 0, Height: > 0 } && startingSampleTexture == null)
        {
            RectI startingRectI = (RectI)startingRect.Round().Inflate(brushNode.TargetOversample.Value);
            requiresStartingSampleTexture |= requiresTargetSampleTexture && !brushNode.AllowSampleStacking.Value;
            if (requiresStartingSampleTexture)
            {
                startingSampleTexture =
                    UpdateSurfaceUnderRect(StartingStampCacheId, target, startingRectI, colorSpace, false);
            }
        }

        if (target != null)
        {
            requiresLatestFullTexture |= requiresTargetFullTexture && brushNode.AllowSampleStacking.Value;
            if (requiresLatestFullTexture)
            {
                latestFullTexture = UpdateFullTexture(target, colorSpace, true);
            }

            requiresStartingFullTexture |= requiresTargetFullTexture && !brushNode.AllowSampleStacking.Value;
            if (requiresStartingFullTexture && startingFullTexture == null)
            {
                startingFullTexture = UpdateFullTexture(target, colorSpace, false);
            }
        }

        BrushRenderContext context = new BrushRenderContext(
            null, frameTime, ChunkResolution.Full,
            brushNode.FitToStrokeSize.NonOverridenValue
                ? ((RectI)rect.RoundOutwards()).Size
                : target?.CommittedSize ?? VecI.Zero,
            target?.CommittedSize ?? VecI.Zero,
            colorSpace, samplingOptions, brushData,
            targetSampleUnderRect, latestSampleUnderRect, rect.TopLeft, startingSampleTexture, startingRect.TopLeft,
            startingFullTexture, latestFullTexture, brushData.BrushGraph,
            startPos, lastPos, stamps, nextRenderId)
        {
            PointerInfo = pointerInfo with { PositionOnCanvas = point },
            EditorData = shouldErase
                ? new EditorData(editorData.PrimaryColor.WithAlpha(255), editorData.SecondaryColor)
                : editorData,
            KeyboardInfo = keyboardInfo,
            DryRun = true
        };

        // Evaluate shape without painting if no target
        if (target == null)
        {
            brushData.BrushGraph.Execute(brushNode, context);
            return;
        }

        if ((requiresLatestSampleTexture || requiresTargetSampleTexture) && brushNode.VectorShape.Value != null)
        {
            brushData.BrushGraph.Execute(brushNode, context);

            using var shape = brushNode.VectorShape.Value.ToPath(true);
            EvaluateShape(brushNode.AutoPosition.Value, shape, brushNode.VectorShape.Value, rect,
                brushNode.SnapToPixels.Value, brushNode.FitToStrokeSize.Value, brushNode.Pressure.Value);

            if (shape.Bounds is { Width: > 0, Height: > 0 })
            {
                //context.TargetSampledTexture?.Dispose();
                RectI size = (RectI)shape.TightBounds.Round().Inflate(brushNode.TargetOversample.Value);
                targetSampleUnderRect = UpdateSurfaceUnderRect(TargetStampCacheId, target,
                    size, colorSpace,
                    brushNode.AllowSampleStacking.Value);
                context.TargetSampleTexture = targetSampleUnderRect;
                if (!brushNode.AllowSampleStacking.Value && requiresLatestSampleTexture)
                {
                    latestSampleUnderRect = UpdateSurfaceUnderRect(LatestStampCacheId, target, size, colorSpace, true);
                }
                else
                {
                    latestSampleUnderRect = targetSampleUnderRect;
                }

                context.LatestSampledTexture = latestSampleUnderRect;
                context.RenderOutputSize = ((RectI)shape.TightBounds.Round()).Size;
                context.GraphCacheId = nextRenderId + 1;
            }
        }

        var previous = brushNode.Previous.Value;
        while (previous != null)
        {
            var data = new BrushData(previous, brushData.TargetBrushNodeId)
            {
                AntiAliasing = brushData.AntiAliasing, StrokeWidth = brushData.StrokeWidth
            };

            var previousBrushNode = previous.AllNodes.FirstOrDefault(x => x is BrushOutputNode) as BrushOutputNode;
            PaintBrush(accumulationBuffer, data, point, previousBrushNode, context, rect, flipX, flipY);
            previous = previousBrushNode?.Previous.Value;
        }

        PaintBrush(accumulationBuffer, brushData, point, brushNode, context, rect, flipX, flipY);
    }

    private void PaintBrush(ChunkyImage target, BrushData brushData, VecD point, BrushOutputNode brushNode,
        BrushRenderContext context, RectD rect, bool flipX, bool flipY)
    {
        context.DryRun = false;
        brushData.BrushGraph.Execute(brushNode, context);

        var vectorShape = brushNode.VectorShape.Value;
        if (vectorShape == null)
        {
            return;
        }

        bool autoPosition = brushNode.AutoPosition.Value;
        bool fitToStrokeSize = brushNode.FitToStrokeSize.Value;
        float pressure = brushNode.Pressure.Value;
        var content = brushNode.Content.Value;
        var contentTexture = brushNode.ContentTexture;
        bool antiAliasing = brushData.AntiAliasing;
        var fill = brushNode.Fill.Value;
        Matrix3X3 fillTransform = brushNode.FillTransform.Value;
        Matrix3X3 strokeTransform = brushNode.StrokeTransform.Value;
        var stroke = brushNode.Stroke.Value;
        bool snapToPixels = brushNode.SnapToPixels.Value;
        bool canReuseStamps = brushNode.CanReuseStamps.Value;
        Blender? stampBlender = brushNode.UseCustomStampBlender.Value ? brushNode.LastStampBlender : null;
        Matrix3X3 transform = brushNode.ContentTransform.Value;
        //Blender? imageBlender = brushNode.UseCustomImageBlender.Value ? brushNode.LastImageBlender : null;

        if (fill != null)
            fill.Transform = fillTransform;

        if (stroke != null)
            stroke.Transform = strokeTransform;

        if (PaintBrush(target, autoPosition, vectorShape, rect, fitToStrokeSize, pressure, content, contentTexture,
                stampBlender, brushNode.StampBlendMode.Value, antiAliasing, fill, stroke, snapToPixels, canReuseStamps,
                transform, flipX, flipY))
        {
            lastPos = point;
            stamps++;
        }
    }

    public bool PaintBrush(ChunkyImage target, bool autoPosition, ShapeVectorData vectorShape,
        RectD rect, bool fitToStrokeSize, float pressure, Painter? content,
        Texture? contentTexture, Blender? blender, DrawingApiBlendMode blendMode, bool antiAliasing, Paintable fill,
        Paintable stroke,
        bool snapToPixels, bool canReuseStamps, Matrix3X3 transform, bool flipX, bool flipY)
    {
        using var path = vectorShape.ToPath(true);
        if (path == null)
        {
            return false;
        }

        if (flipX)
        {
            path.Transform(Matrix3X3.CreateScale(-1, 1, (float)rect.Center.X, (float)rect.Center.Y));
        }

        if (flipY)
        {
            path.Transform(Matrix3X3.CreateScale(1, -1, (float)rect.Center.X, (float)rect.Center.Y));
        }

        EvaluateShape(autoPosition, path, vectorShape, rect, snapToPixels, fitToStrokeSize, pressure);

        StrokeCap strokeCap = StrokeCap.Butt;
        PaintStyle strokeStyle = PaintStyle.Fill;

        var paintable = fill;

        if (fill != null && fill.AnythingVisible)
        {
            strokeStyle = PaintStyle.Fill;
        }
        else
        {
            strokeStyle = PaintStyle.Stroke;
            paintable = stroke;
        }

        Matrix3X3 paintTransform = Matrix3X3.Identity;

        if (vectorShape is PathVectorData pathData)
        {
            strokeCap = pathData.StrokeLineCap;
        }

        if (paintable is { AnythingVisible: true })
        {
            VecD paintableCenter = paintable.LocalBounds.Center;
            if (paintable is TexturePaintable texturePaintable)
            {
                texturePaintable.SamplingOptions = antiAliasing ? SamplingOptions.Bilinear : SamplingOptions.Default;

                paintableCenter = texturePaintable.LocalBounds.Center;
            }
            else if (paintable is GradientPaintable)
            {
                paintableCenter = rect.Center;
            }

            if (flipX)
            {
                paintTransform = Matrix3X3.CreateScale(-1, 1, (float)paintableCenter.X, (float)paintableCenter.Y);
            }

            if (flipY)
            {
                paintTransform =
                    paintTransform.PostConcat(Matrix3X3.CreateScale(1, -1, (float)paintableCenter.X,
                        (float)paintableCenter.Y));
            }

            if (blender != null)
            {
                target.EnqueueNonMirroredDrawPath(path, paintable, vectorShape.StrokeWidth,
                    strokeCap, blender, strokeStyle, antiAliasing, null, paintTransform);
            }
            else
            {
                var paintableOp = paintable.Clone();
                paintableOp.ApplyOpacity(0.2);
                target.EnqueueNonMirroredDrawPath(path, paintableOp, vectorShape.StrokeWidth,
                    strokeCap, blendMode, strokeStyle, antiAliasing, null, paintTransform);
            }
        }

        if (fill is { AnythingVisible: true } && stroke is { AnythingVisible: true })
        {
            strokeStyle = PaintStyle.Stroke;
            if (blender != null)
            {
                target.EnqueueNonMirroredDrawPath(path, stroke, vectorShape.StrokeWidth,
                    strokeCap, blender, strokeStyle, antiAliasing, null, paintTransform);
            }
            else
            {
                target.EnqueueNonMirroredDrawPath(path, stroke, vectorShape.StrokeWidth,
                    strokeCap, blendMode, strokeStyle, antiAliasing, null, paintTransform);
            }
        }

        if (content != null)
        {
            if (contentTexture != null)
            {
                TexturePaintable brushPaintable;
                if (canReuseStamps)
                {
                    if (lastCachedTexturePaintableSize != contentTexture.Size || lastCachedTexturePaintable == null ||
                        lastCachedTransform != transform)
                    {
                        lastCachedTexturePaintable?.Dispose();
                        lastCachedTexturePaintable = new TexturePaintable(new Texture(contentTexture), false);
                        lastCachedTexturePaintableSize = contentTexture.Size;
                        lastCachedTransform = transform;
                    }

                    brushPaintable = lastCachedTexturePaintable;
                }
                else
                {
                    brushPaintable = new TexturePaintable(new Texture(contentTexture), true);
                }

                brushPaintable.SamplingOptions = antiAliasing ? SamplingOptions.Bilinear : SamplingOptions.Default;

                if (blender != null)
                {
                    target.EnqueueNonMirroredDrawPath(path, brushPaintable, vectorShape.StrokeWidth,
                        StrokeCap.Butt, blender, PaintStyle.Fill, antiAliasing, null, paintTransform);
                }
                else
                {
                    target.EnqueueNonMirroredDrawPath(path, brushPaintable, vectorShape.StrokeWidth,
                        StrokeCap.Butt, blendMode, PaintStyle.Fill, antiAliasing, null, paintTransform);
                }
            }
        }

        return true;
    }

    private Texture UpdateFullTexture(ChunkyImage target, ColorSpace colorSpace, bool sampleLatest)
    {
        var size = sampleLatest ? target.LatestSize : target.CommittedSize;
        var texture = cache.RequestTexture(sampleLatest ? FullTextureLatestCacheId : FullTextureCacheId, size,
            colorSpace);
        if (!sampleLatest)
        {
            target.DrawCommittedRegionOn(new RectI(VecI.Zero, size), ChunkResolution.Full,
                texture.DrawingSurface.Canvas, VecI.Zero);
            return texture;
        }

        target.DrawMostUpToDateRegionOn(new RectI(VecI.Zero, size), ChunkResolution.Full,
            texture.DrawingSurface.Canvas, VecI.Zero);
        return texture;
    }


    private Texture UpdateSurfaceUnderRect(int cacheId, ChunkyImage target, RectI rect, ColorSpace colorSpace,
        bool sampleLatest)
    {
        var surfaceUnderRect = cache.RequestTexture(cacheId, rect.Size, colorSpace);

        if (sampleLatest)
        {
            target.DrawMostUpToDateRegionOn(rect, ChunkResolution.Full, surfaceUnderRect.DrawingSurface.Canvas,
                VecI.Zero);
        }
        else
        {
            target.DrawCommittedRegionOn(rect, ChunkResolution.Full, surfaceUnderRect.DrawingSurface.Canvas, VecI.Zero);
        }

        return surfaceUnderRect;
    }

    private bool GraphUsesConnections(IReadOnlyNodeGraph graph, IReadOnlyNode brushNode,
        Func<IBrushSampleTextureNode, IReadOnlyCollection<IInputProperty>> getConnections, Dictionary<Guid, bool> cache)
    {
        if (cache.TryGetValue(brushNode.Id, out bool uses))
        {
            return uses;
        }

        bool usesInput = GraphUsesInput(graph, brushNode, getConnections);

        cache[brushNode.Id] = usesInput;

        return usesInput;
    }

    private bool GraphUsesInput(IReadOnlyNodeGraph graph, IReadOnlyNode brushNode,
        Func<IBrushSampleTextureNode, IReadOnlyCollection<IInputProperty>> getConnections)
    {
        foreach (var node in graph.AllNodes)
        {
            if (node is IBrushSampleTextureNode brushSampleTextureNode)
            {
                var connections = getConnections(brushSampleTextureNode);
                if (connections.Count == 0)
                {
                    continue;
                }

                foreach (var connection in connections)
                {
                    bool found = false;
                    connection.Connection.Node.TraverseForwards(x =>
                    {
                        if (x == brushNode)
                        {
                            found = true;
                            return false;
                        }

                        return true;
                    });

                    if (found)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public VectorPath? EvaluateShape(VecD point, BrushData brushData)
    {
        return EvaluateShape(point, brushData,
            brushData.BrushGraph.AllNodes.FirstOrDefault(x => x is BrushOutputNode) as BrushOutputNode);
    }

    public VectorPath? EvaluateShape(VecD point, BrushData brushData, BrushOutputNode brushNode)
    {
        if (brushNode == null)
            return null;

        var vectorShape = brushNode.VectorShape.Value;
        if (vectorShape == null)
        {
            return null;
        }

        float strokeWidth = brushData.StrokeWidth;
        var rect = new RectD(point - new VecD((strokeWidth / 2f)), new VecD(strokeWidth));

        bool autoPosition = brushNode.AutoPosition.Value;
        bool fitToStrokeSize = brushNode.FitToStrokeSize.Value;
        float pressure = brushNode.Pressure.Value;
        bool snapToPixels = brushNode.SnapToPixels.Value;

        if (snapToPixels)
        {
            rect = (RectD)(new RectI((VecI)point - new VecI((int)(strokeWidth / 2f)), new VecI((int)strokeWidth)));
        }

        var path = vectorShape.ToPath(true);
        if (path == null)
        {
            return null;
        }

        EvaluateShape(autoPosition, path, vectorShape, rect, snapToPixels, fitToStrokeSize, pressure);

        return path;
    }

    private static void EvaluateShape(bool autoPosition, VectorPath path, ShapeVectorData vectorShape, RectD rect,
        bool snapToPixels, bool fitToStrokeSize, float pressure)
    {
        if (fitToStrokeSize)
        {
            VecD scale = new VecD(rect.Size.X / (float)vectorShape.GeometryAABB.Width,
                rect.Size.Y / (float)vectorShape.GeometryAABB.Height);
            if (scale.IsNaNOrInfinity())
            {
                scale = VecD.Zero;
            }

            VecD uniformScale = new VecD(Math.Min(scale.X, scale.Y) * pressure);
            VecD center = autoPosition ? rect.Center : vectorShape.GeometryAABB.Center;

            path.Transform(Matrix3X3.CreateScale((float)uniformScale.X, (float)uniformScale.Y, (float)center.X,
                (float)center.Y));

            if (snapToPixels)
            {
                // stretch to pixels
                path.Transform(Matrix3X3.CreateScale(
                    (float)(Math.Round(path.TightBounds.Width) / path.TightBounds.Width),
                    (float)(Math.Round(path.TightBounds.Height) / path.TightBounds.Height),
                    (float)center.X,
                    (float)center.Y));
            }
        }
        else
        {
            Matrix3X3 pressureScale = Matrix3X3.CreateScale(pressure, pressure, (float)rect.Center.X,
                (float)rect.Center.Y);
            path.Transform(pressureScale);
        }

        if (autoPosition)
        {
            path.Offset(vectorShape.TransformedAABB.Pos - vectorShape.GeometryAABB.Pos);
            path.Offset(rect.Center - path.TightBounds.Center);

            if (snapToPixels)
            {
                path.Offset(
                    new VecD(Math.Round(path.TightBounds.Pos.X) - path.TightBounds.Pos.X,
                        Math.Round(path.TightBounds.Pos.Y) - path.TightBounds.Pos.Y));
            }
        }
    }

    private void ResetStartingTextures()
    {
        startingFullTexture = null;
        startingSampleTexture = null;
    }

    public void Dispose()
    {
        cache.Dispose();
        lastCachedTexturePaintable?.Dispose();
        ResetStartingTextures();
    }
}
