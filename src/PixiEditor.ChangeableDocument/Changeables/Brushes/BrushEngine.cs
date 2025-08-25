using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
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

namespace PixiEditor.ChangeableDocument.Changeables.Brushes;

internal class BrushEngine
{
    private TextureCache cache = new();
    public void ExecuteBrush(ChunkyImage target, BrushData brushData, VecI point, KeyFrameTime frameTime, ColorSpace cs, SamplingOptions samplingOptions, PointerInfo pointerInfo, EditorData editorData)
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

        using var texture = Texture.ForDisplay(size);
        var surfaceUnderRect = UpdateSurfaceUnderRect(target, rect, colorSpace);
        BrushRenderContext context = new BrushRenderContext(texture.DrawingSurface, frameTime, ChunkResolution.Full, size, size,
            colorSpace, samplingOptions, brushData, surfaceUnderRect) { PointerInfo = pointerInfo, EditorData = editorData };

        brushData.BrushGraph.Execute(brushNode, context);

        var vectorShape = brushNode.VectorShape.Value;
        if (vectorShape == null)
        {
            return;
        }

        var path = vectorShape.ToPath(true);
        if (path == null)
        {
            return;
        }


        path.Offset(vectorShape.TransformedAABB.Pos - vectorShape.GeometryAABB.Pos);
        path.Offset(rect.Center - path.Bounds.Center);

        if (brushNode.FitToStrokeSize.Value)
        {
            VecD scale = new VecD(rect.Size.X / (float)path.Bounds.Width, rect.Size.Y / (float)path.Bounds.Height);
            if (scale.IsNaNOrInfinity())
            {
                scale = VecD.Zero;
            }

            VecD uniformScale = new VecD(Math.Min(scale.X, scale.Y));
            path.Transform(Matrix3X3.CreateScale((float)uniformScale.X, (float)uniformScale.Y, (float)rect.Center.X,
                (float)rect.Center.Y));
        }

        var pressure = brushNode.Pressure.Value;
        Matrix3X3 pressureScale = Matrix3X3.CreateScale(pressure, pressure, (float)rect.Center.X,
            (float)rect.Center.Y);
        path.Transform(pressureScale);

        if (brushNode.Content.Value != null)
        {
            var brushTexture = brushNode.ContentTexture;
            if (brushTexture != null)
            {
                TexturePaintable brushTexturePaintable = new(brushTexture);
                target.EnqueueDrawPath(path, brushTexturePaintable, vectorShape.StrokeWidth,
                    StrokeCap.Butt, brushData.BlendMode, PaintStyle.Fill, brushData.AntiAliasing);
                return;
            }
        }

        StrokeCap strokeCap = StrokeCap.Butt;
        PaintStyle strokeStyle = PaintStyle.Fill;

        var fill = brushNode.Fill.Value;
        var stroke = brushNode.Stroke.Value;
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
            strokeCap, brushData.BlendMode, strokeStyle, brushData.AntiAliasing);

        if (fill is { AnythingVisible: true } && stroke is { AnythingVisible: true })
        {
            strokeStyle = PaintStyle.Stroke;
            target.EnqueueDrawPath(path, stroke, vectorShape.StrokeWidth,
                strokeCap, brushData.BlendMode, strokeStyle, brushData.AntiAliasing);
        }
    }

    private Texture UpdateSurfaceUnderRect(ChunkyImage target, RectI rect, ColorSpace colorSpace)
    {
        var surfaceUnderRect = cache.RequestTexture(0, rect.Size, colorSpace);

        target.DrawCommittedRegionOn(rect, ChunkResolution.Full, surfaceUnderRect.DrawingSurface, VecI.Zero);
        return surfaceUnderRect;
    }
}
