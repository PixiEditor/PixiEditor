using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Brushes;

internal class BrushEngine
{
    public void ExecuteBrush(ChunkyImage target, BrushData brushData, VecI point, KeyFrameTime frameTime)
    {
        ExecuteVectorShapeBrush(target, brushData, point, frameTime);
    }

    private void ExecuteVectorShapeBrush(ChunkyImage target, BrushData brushData, VecI point, KeyFrameTime frameTime)
    {
        var brushNode = brushData.BrushGraph.AllNodes.FirstOrDefault(x => x is BrushOutputNode) as BrushOutputNode;
        if (brushNode == null)
        {
            return;
        }

        VecI size = new VecI((int)float.Ceiling(brushData.StrokeWidth));
        using var texture = Texture.ForDisplay(size);
        RenderContext context = new RenderContext(texture.DrawingSurface, frameTime, ChunkResolution.Full, size, size,
            ColorSpace.CreateSrgbLinear());
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

        float strokeWidth = brushData.StrokeWidth;
        var rect = new RectI(point - new VecI((int)(strokeWidth / 2f)), new VecI((int)strokeWidth));

        path.Offset(vectorShape.TransformedAABB.Pos - vectorShape.GeometryAABB.Pos);
        path.Offset(rect.Center - path.Bounds.Center);

        if (brushData.FitToStrokeSize)
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

        Matrix3X3 pressureScale = Matrix3X3.CreateScale(brushData.Pressure, brushData.Pressure, (float)rect.Center.X,
            (float)rect.Center.Y);
        path.Transform(pressureScale);

        StrokeCap strokeCap = StrokeCap.Butt;
        PaintStyle strokeStyle = PaintStyle.Fill;

        var fill = brushNode.Fill.Value;
        var stroke = brushNode.Stroke.Value;

        if (fill != null && fill.AnythingVisible)
        {
            strokeStyle = PaintStyle.Fill;
        }
        else
        {
            strokeStyle = PaintStyle.Stroke;
        }

        if (vectorShape is PathVectorData pathData)
        {
            strokeCap = pathData.StrokeLineCap;
        }

        target.EnqueueDrawPath(path, fill, vectorShape.StrokeWidth,
            strokeCap, brushData.BlendMode, strokeStyle, brushData.AntiAliasing);

        if (fill is { AnythingVisible: true } && stroke is { AnythingVisible: true })
        {
            strokeStyle = PaintStyle.Stroke;
            target.EnqueueDrawPath(path, stroke, vectorShape.StrokeWidth,
                strokeCap, brushData.BlendMode, strokeStyle, brushData.AntiAliasing);
        }
    }
}
