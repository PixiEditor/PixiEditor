using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
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
    private TextureCache cache = new();
    private VecD lastPos;
    private VecD startPos;
    private int lastAppliedPointIndex = -1;

    private bool drawnOnce = false;

    public void ResetState()
    {
        lastAppliedPointIndex = -1;
        drawnOnce = false;
    }

    public void ExecuteBrush(ChunkyImage target, BrushData brushData, List<VecD> points, KeyFrameTime frameTime,
        ColorSpace cs, SamplingOptions samplingOptions, PointerInfo pointerInfo, KeyboardInfo keyboardInfo,
        EditorData editorData)
    {
        if (brushData.BrushGraph == null)
        {
            return;
        }

        if (brushData.BrushGraph.AllNodes.FirstOrDefault(x => x is BrushOutputNode) is not BrushOutputNode brushNode)
        {
            return;
        }

        float strokeWidth = brushData.StrokeWidth;
        float spacing = brushNode.Spacing.Value / 100f;

        float spacingPixels = (strokeWidth * pointerInfo.Pressure) * spacing;

        for (int i = Math.Max(lastAppliedPointIndex, 0); i < points.Count; i++)
        {
            var point = points[i];
            if (VecD.Distance(lastPos, point) < spacingPixels)
                continue;

            ExecuteVectorShapeBrush(target, brushNode, brushData, point, frameTime, cs, samplingOptions, pointerInfo,
                keyboardInfo,
                editorData);

            lastPos = point;
        }

        lastAppliedPointIndex = points.Count - 1;
    }

    public void ExecuteBrush(ChunkyImage target, BrushData brushData, List<RecordedPoint> points,
        KeyFrameTime frameTime,
        ColorSpace cs, SamplingOptions samplingOptions)
    {
        if (brushData.BrushGraph == null)
        {
            return;
        }

        if (brushData.BrushGraph.AllNodes.FirstOrDefault(x => x is BrushOutputNode) is not BrushOutputNode brushNode)
        {
            return;
        }

        float strokeWidth = brushData.StrokeWidth;
        float spacing = brushNode.Spacing.Value / 100f;

        for (int i = Math.Max(lastAppliedPointIndex, 0); i < points.Count; i++)
        {
            var point = points[i];

            float spacingPixels = (strokeWidth * point.PointerInfo.Pressure) * spacing;
            if (VecD.Distance(lastPos, point.Position) < spacingPixels)
                continue;

            ExecuteVectorShapeBrush(target, brushNode, brushData, point.Position, frameTime, cs, samplingOptions,
                point.PointerInfo,
                point.KeyboardInfo,
                point.EditorData);

            lastPos = point.Position;
        }

        lastAppliedPointIndex = points.Count - 1;
    }


    public void ExecuteBrush(ChunkyImage target, BrushData brushData, VecD point, KeyFrameTime frameTime, ColorSpace cs,
        SamplingOptions samplingOptions, PointerInfo pointerInfo, KeyboardInfo keyboardInfo, EditorData editorData)
    {
        var brushNode = brushData.BrushGraph?.AllNodes?.FirstOrDefault(x => x is BrushOutputNode) as BrushOutputNode;
        if (brushNode == null)
        {
            return;
        }

        ExecuteVectorShapeBrush(target, brushNode, brushData, point, frameTime, cs, samplingOptions, pointerInfo,
            keyboardInfo,
            editorData);
    }

    private void ExecuteVectorShapeBrush(ChunkyImage target, BrushOutputNode brushNode, BrushData brushData, VecD point,
        KeyFrameTime frameTime,
        ColorSpace colorSpace, SamplingOptions samplingOptions,
        PointerInfo pointerInfo, KeyboardInfo keyboardInfo, EditorData editorData)
    {
        bool shouldErase = editorData.PrimaryColor.A == 0;

        var imageBlendMode = shouldErase ? DrawingApiBlendMode.DstOut : brushNode.ImageBlendMode.Value;

        if (!drawnOnce)
        {
            startPos = point;
            lastPos = point;
            drawnOnce = true;
            target.SetBlendMode(imageBlendMode);
        }

        float strokeWidth = brushData.StrokeWidth;
        var rect = new RectD(point - new VecD((strokeWidth / 2f)), new VecD(strokeWidth));
        if (brushNode.SnapToPixels.Value)
        {
            VecI vecIpoint = (VecI)point;
            rect = (RectD)new RectI(vecIpoint - new VecI((int)(strokeWidth / 2f)), new VecI((int)strokeWidth));
        }

        bool requiresSampleTexture = GraphUsesSampleTexture(brushData.BrushGraph, brushNode);
        bool requiresFullTexture = GraphUsesFullTexture(brushData.BrushGraph, brushNode);
        Texture? surfaceUnderRect = null;
        Texture? fullTexture = null;
        Texture texture = null;

        if (brushNode.AlwaysClear.Value)
        {
            target.EnqueueClear();
        }

        if (requiresSampleTexture && rect.Width > 0 && rect.Height > 0)
        {
            surfaceUnderRect = UpdateSurfaceUnderRect(target, (RectI)rect.RoundOutwards(), colorSpace,
                brushNode.AllowSampleStacking.Value);
        }

        if (requiresFullTexture)
        {
            fullTexture = UpdateFullTexture(target, colorSpace, brushNode.AllowSampleStacking.Value);
        }

        BrushRenderContext context = new BrushRenderContext(
            texture?.DrawingSurface.Canvas, frameTime, ChunkResolution.Full,
            brushNode.FitToStrokeSize.NonOverridenValue ? ((RectI)rect.RoundOutwards()).Size : target.CommittedSize,
            target.CommittedSize,
            colorSpace, samplingOptions, brushData,
            surfaceUnderRect, fullTexture, brushData.BrushGraph,
            startPos, lastPos)
        {
            PointerInfo = pointerInfo,
            EditorData = shouldErase
                ? new EditorData(editorData.PrimaryColor.WithAlpha(255), editorData.SecondaryColor)
                : editorData,
            KeyboardInfo = keyboardInfo
        };


        if (requiresSampleTexture && brushNode.VectorShape.Value != null)
        {
            brushData.BrushGraph.Execute(brushNode, context);

            using var shape = brushNode.VectorShape.Value.ToPath(true);
            EvaluateShape(brushNode.AutoPosition.Value, shape, brushNode.VectorShape.Value, rect,
                brushNode.SnapToPixels.Value, brushNode.FitToStrokeSize.Value, brushNode.Pressure.Value);

            if (shape.Bounds is { Width: > 0, Height: > 0 })
            {
                context.TargetSampledTexture?.Dispose();
                surfaceUnderRect = UpdateSurfaceUnderRect(target, (RectI)shape.TightBounds.RoundOutwards(), colorSpace,
                    brushNode.AllowSampleStacking.Value);
                context.TargetSampledTexture = surfaceUnderRect;
                context.RenderOutputSize = ((RectI)shape.TightBounds.RoundOutwards()).Size;
            }
        }

        var previous = brushNode.Previous.Value;
        while (previous != null)
        {
            var data = new BrushData(previous)
            {
                AntiAliasing = brushData.AntiAliasing, StrokeWidth = brushData.StrokeWidth,
            };

            var previousBrushNode = previous.AllNodes.FirstOrDefault(x => x is BrushOutputNode) as BrushOutputNode;
            PaintBrush(target, data, point, previousBrushNode, context, rect);
            previous = previousBrushNode?.Previous.Value;
        }

        PaintBrush(target, brushData, point, brushNode, context, rect);
    }

    private void PaintBrush(ChunkyImage target, BrushData brushData, VecD point, BrushOutputNode brushNode,
        BrushRenderContext context, RectD rect)
    {
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
        var stroke = brushNode.Stroke.Value;
        bool snapToPixels = brushNode.SnapToPixels.Value;

        if (PaintBrush(target, autoPosition, vectorShape, rect, fitToStrokeSize, pressure, content, contentTexture,
                brushNode.StampBlendMode.Value, antiAliasing, fill, stroke, snapToPixels))
        {
            lastPos = point;
        }
    }

    public bool PaintBrush(ChunkyImage target, bool autoPosition, ShapeVectorData vectorShape,
        RectD rect, bool fitToStrokeSize, float pressure, Painter? content,
        Texture? contentTexture, DrawingApiBlendMode blendMode, bool antiAliasing, Paintable fill, Paintable stroke,
        bool snapToPixels)
    {
        var path = vectorShape.ToPath(true);
        if (path == null)
        {
            return false;
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

        if (vectorShape is PathVectorData pathData)
        {
            strokeCap = pathData.StrokeLineCap;
        }

        if (paintable is { AnythingVisible: true })
        {
            target.EnqueueDrawPath(path, paintable, vectorShape.StrokeWidth,
                strokeCap, blendMode, strokeStyle, antiAliasing, null);
        }

        if (fill is { AnythingVisible: true } && stroke is { AnythingVisible: true })
        {
            strokeStyle = PaintStyle.Stroke;
            target.EnqueueDrawPath(path, stroke, vectorShape.StrokeWidth,
                strokeCap, blendMode, strokeStyle, antiAliasing, null);
        }

        if (content != null)
        {
            if (contentTexture != null)
            {
                TexturePaintable brushTexturePaintable = new(new Texture(contentTexture), true);
                target.EnqueueDrawPath(path, brushTexturePaintable, vectorShape.StrokeWidth,
                    StrokeCap.Butt, blendMode, PaintStyle.Fill, antiAliasing, null);
            }
        }

        return true;
    }

    private Texture UpdateFullTexture(ChunkyImage target, ColorSpace colorSpace, bool sampleLatest)
    {
        var texture = cache.RequestTexture(1, target.LatestSize, colorSpace);
        if (!sampleLatest)
        {
            target.DrawCommittedRegionOn(new RectI(VecI.Zero, target.LatestSize), ChunkResolution.Full,
                texture.DrawingSurface.Canvas, VecI.Zero);
            return texture;
        }

        target.DrawMostUpToDateRegionOn(new RectI(VecI.Zero, target.LatestSize), ChunkResolution.Full,
            texture.DrawingSurface.Canvas, VecI.Zero);
        return texture;
    }

    private Texture UpdateSurfaceUnderRect(ChunkyImage target, RectI rect, ColorSpace colorSpace, bool sampleLatest)
    {
        var surfaceUnderRect = cache.RequestTexture(0, rect.Size, colorSpace);

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

    private bool GraphUsesSampleTexture(IReadOnlyNodeGraph graph, IReadOnlyNode brushNode)
    {
        return GraphUsesInput(graph, brushNode, node => node.TargetSampleTexture.Connections);
    }

    private bool GraphUsesFullTexture(IReadOnlyNodeGraph graph, IReadOnlyNode brushNode)
    {
        return GraphUsesInput(graph, brushNode, node => node.TargetFullTexture.Connections);
    }

    private bool GraphUsesInput(IReadOnlyNodeGraph graph, IReadOnlyNode brushNode,
        Func<IBrushSampleTextureNode, IReadOnlyCollection<IInputProperty>> getConnections)
    {
        var sampleTextureNodes = graph.AllNodes.Where(x => x is IBrushSampleTextureNode).ToList();
        if (sampleTextureNodes.Count == 0)
        {
            return false;
        }

        foreach (var node in sampleTextureNodes)
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
            VecD scale = new VecD(rect.Size.X / (float)path.TightBounds.Width, rect.Size.Y / (float)path.TightBounds.Height);
            if (scale.IsNaNOrInfinity())
            {
                scale = VecD.Zero;
            }

            VecD uniformScale = new VecD(Math.Min(scale.X, scale.Y));
            VecD center = autoPosition ? rect.Center : vectorShape.TransformedAABB.Center;

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


        Matrix3X3 pressureScale = Matrix3X3.CreateScale(pressure, pressure, (float)rect.Center.X,
            (float)rect.Center.Y);
        path.Transform(pressureScale);
    }

    public void Dispose()
    {
        cache.Dispose();
    }
}
