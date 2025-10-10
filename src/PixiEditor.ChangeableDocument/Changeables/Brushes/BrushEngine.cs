using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using DrawingApiBlendMode = Drawie.Backend.Core.Surfaces.BlendMode;

namespace PixiEditor.ChangeableDocument.Changeables.Brushes;

internal class BrushEngine
{
    private TextureCache cache = new();
    private VecF lastPos;
    private int lastAppliedPointIndex = -1;

    public void ExecuteBrush(ChunkyImage target, BrushData brushData, List<VecI> points, KeyFrameTime frameTime,
        ColorSpace cs, SamplingOptions samplingOptions, PointerInfo pointerInfo, EditorData editorData)
    {
        float strokeWidth = brushData.StrokeWidth;
        float spacing = brushData.Spacing;

        float spacingPixels = (strokeWidth * pointerInfo.Pressure) * spacing;

        for (int i = Math.Max(lastAppliedPointIndex, 0); i < points.Count; i++)
        {
            var point = points[i];
            if (VecF.Distance(lastPos, point) < spacingPixels)
                continue;

            ExecuteVectorShapeBrush(target, brushData, point, frameTime, cs, samplingOptions, pointerInfo, editorData);

            lastPos = point;
        }

        lastAppliedPointIndex = points.Count - 1;
    }

    public void ExecuteBrush(ChunkyImage target, BrushData brushData, VecI point, KeyFrameTime frameTime, ColorSpace cs,
        SamplingOptions samplingOptions, PointerInfo pointerInfo, EditorData editorData)
    {
        ExecuteVectorShapeBrush(target, brushData, point, frameTime, cs, samplingOptions, pointerInfo, editorData);
    }

    private void ExecuteVectorShapeBrush(ChunkyImage target, BrushData brushData, VecI point, KeyFrameTime frameTime,
        ColorSpace colorSpace, SamplingOptions samplingOptions,
        PointerInfo pointerInfo, EditorData editorData)
    {
        if (brushData.BrushGraph == null)
        {
            return;
        }

        var brushNode = brushData.BrushGraph.AllNodes.FirstOrDefault(x => x is BrushOutputNode) as BrushOutputNode;
        if (brushNode == null)
        {
            return;
        }

        float strokeWidth = brushData.StrokeWidth;
        var rect = new RectI(point - new VecI((int)(strokeWidth / 2f)), new VecI((int)strokeWidth));
        VecI size = new VecI((int)float.Ceiling(brushData.StrokeWidth));

        bool requiresSampleTexture = GraphUsesSampleTexture(brushData.BrushGraph, brushNode);
        bool requiresFullTexture = GraphUsesFullTexture(brushData.BrushGraph, brushNode);
        Texture? surfaceUnderRect = null;
        Texture? fullTexture = null;
        Texture texture = null;

        if (requiresSampleTexture)
        {
            surfaceUnderRect = UpdateSurfaceUnderRect(target, rect, colorSpace, brushNode.AllowSampleStacking.Value);
        }

        if (requiresFullTexture)
        {
            fullTexture = UpdateFullTexture(target, colorSpace, brushNode.AllowSampleStacking.Value);
        }

        BrushRenderContext context = new BrushRenderContext(texture?.DrawingSurface, frameTime, ChunkResolution.Full,
            size, size,
            colorSpace, samplingOptions, brushData, surfaceUnderRect, fullTexture, brushData.BrushGraph)
        {
            PointerInfo = pointerInfo, EditorData = editorData
        };

        brushData.BrushGraph.Execute(brushNode, context);

        var vectorShape = brushNode.VectorShape.Value;
        if (vectorShape == null)
        {
            return;
        }

        bool autoPosition = brushNode.AutoPosition.Value;
        bool fitToStrokeSize = brushNode.FitToStrokeSize.Value;
        float pressure = brushNode.Pressure.Value;
        var blendMode = RenderContext.GetDrawingBlendMode(brushNode.BlendMode.Value);
        var content = brushNode.Content.Value;
        var contentTexture = brushNode.ContentTexture;
        bool antiAliasing = brushData.AntiAliasing;
        var fill = brushNode.Fill.Value;
        var stroke = brushNode.Stroke.Value;

        PaintBrush(target, autoPosition, vectorShape, rect, fitToStrokeSize, pressure, content, contentTexture, blendMode, antiAliasing, fill, stroke);
    }

    public static void PaintBrush(ChunkyImage target, bool autoPosition, ShapeVectorData vectorShape,
        RectI rect, bool fitToStrokeSize, float pressure, Painter? content,
        Texture? contentTexture, DrawingApiBlendMode blendMode, bool antiAliasing, Paintable fill, Paintable stroke)
    {
        var path = vectorShape.ToPath(true);
        if (path == null)
        {
            return;
        }

        if (autoPosition)
        {
            path.Offset(vectorShape.TransformedAABB.Pos - vectorShape.GeometryAABB.Pos);
            path.Offset(rect.Center - path.Bounds.Center);
        }

        if (fitToStrokeSize)
        {
            VecD scale = new VecD(rect.Size.X / (float)path.Bounds.Width, rect.Size.Y / (float)path.Bounds.Height);
            if (scale.IsNaNOrInfinity())
            {
                scale = VecD.Zero;
            }

            VecD uniformScale = new VecD(Math.Min(scale.X, scale.Y));
            VecD center = autoPosition ? rect.Center : vectorShape.TransformedAABB.Center;
            path.Transform(Matrix3X3.CreateScale((float)uniformScale.X, (float)uniformScale.Y, (float)center.X,
                (float)center.Y));
        }

        Matrix3X3 pressureScale = Matrix3X3.CreateScale(pressure, pressure, (float)rect.Center.X,
            (float)rect.Center.Y);
        path.Transform(pressureScale);

        if (content != null)
        {
            if (contentTexture != null)
            {
                TexturePaintable brushTexturePaintable = new(new Texture(contentTexture), true);
                target.EnqueueDrawPath(path, brushTexturePaintable, vectorShape.StrokeWidth,
                    StrokeCap.Butt, blendMode, PaintStyle.Fill, antiAliasing, null);
                return;
            }
        }

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

        target.EnqueueDrawPath(path, paintable, vectorShape.StrokeWidth,
            strokeCap, blendMode, strokeStyle, antiAliasing, null);

        if (fill is { AnythingVisible: true } && stroke is { AnythingVisible: true })
        {
            strokeStyle = PaintStyle.Stroke;
            target.EnqueueDrawPath(path, stroke, vectorShape.StrokeWidth,
                strokeCap, blendMode, strokeStyle, antiAliasing, null);
        }
    }

    private Texture UpdateFullTexture(ChunkyImage target, ColorSpace colorSpace, bool sampleLatest)
    {
        var texture = cache.RequestTexture(1, target.LatestSize, colorSpace);
        if (!sampleLatest)
        {
            target.DrawCommittedRegionOn(new RectI(VecI.Zero, target.LatestSize), ChunkResolution.Full, texture.DrawingSurface, VecI.Zero);
            return texture;
        }

        target.DrawMostUpToDateRegionOn(new RectI(VecI.Zero, target.LatestSize), ChunkResolution.Full, texture.DrawingSurface, VecI.Zero);
        return texture;
    }

    private Texture UpdateSurfaceUnderRect(ChunkyImage target, RectI rect, ColorSpace colorSpace, bool sampleLatest)
    {
        var surfaceUnderRect = cache.RequestTexture(0, rect.Size, colorSpace);

        if (sampleLatest)
        {
            target.DrawMostUpToDateRegionOn(rect, ChunkResolution.Full, surfaceUnderRect.DrawingSurface, VecI.Zero);
        }
        else
        {
            target.DrawCommittedRegionOn(rect, ChunkResolution.Full, surfaceUnderRect.DrawingSurface, VecI.Zero);
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

    private bool GraphUsesInput(IReadOnlyNodeGraph graph, IReadOnlyNode brushNode, Func<IBrushSampleTextureNode, IReadOnlyCollection<IInputProperty>> getConnections)
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

}
