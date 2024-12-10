using System.Diagnostics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class LineVectorData(VecD startPos, VecD pos) : ShapeVectorData, IReadOnlyLineData
{
    public VecD Start { get; set; } = startPos; // Relative to the document top left
    public VecD End { get; set; } = pos; // Relative to the document top left

    public VecD TransformedStart
    {
        get => TransformationMatrix.MapPoint(Start);
        set => Start = TransformationMatrix.Invert().MapPoint(value);
    }

    public VecD TransformedEnd
    {
        get => TransformationMatrix.MapPoint(End);
        set => End = TransformationMatrix.Invert().MapPoint(value);
    }

    public override RectD GeometryAABB
    {
        get
        {
            var dir = (End - Start).Normalize();
            var cross = new VecD(-dir.Y, dir.X);
            VecD offset = cross * StrokeWidth / 2;

            VecD topLeft = Start + offset;
            VecD bottomRight = End - offset;
            VecD bottomLeft = Start - offset;
            VecD topRight = End + offset;

            ShapeCorners corners = new ShapeCorners()
            {
                TopLeft = topLeft, BottomRight = bottomRight, BottomLeft = bottomLeft, TopRight = topRight
            };

            return corners.AABBBounds;
        }
    }

    public override RectD VisualAABB => GeometryAABB;

    public override ShapeCorners TransformationCorners => new ShapeCorners(GeometryAABB)
        .WithMatrix(TransformationMatrix);

    public override void RasterizeGeometry(DrawingSurface drawingSurface)
    {
        Rasterize(drawingSurface, false);
    }

    public override void RasterizeTransformed(DrawingSurface drawingSurface)
    {
        Rasterize(drawingSurface, true);
    }

    private void Rasterize(DrawingSurface drawingSurface, bool applyTransform)
    {
        int num = 0;
        if (applyTransform)
        {
            num = drawingSurface.Canvas.Save();
            ApplyTransformTo(drawingSurface);
        }

        using Paint paint = new Paint() { IsAntiAliased = true };

        paint.Color = StrokeColor;
        paint.Style = PaintStyle.Stroke;
        paint.StrokeWidth = StrokeWidth;

        drawingSurface.Canvas.DrawLine(Start, End, paint);

        if (applyTransform)
        {
            drawingSurface.Canvas.RestoreToCount(num);
        }
    }

    public override bool IsValid()
    {
        return Start != End;
    }

    public override int GetCacheHash()
    {
        return HashCode.Combine(Start, End, StrokeColor, StrokeWidth, TransformationMatrix);
    }

    public override int CalculateHash()
    {
        return GetCacheHash();
    }

    public override object Clone()
    {
        return new LineVectorData(Start, End)
        {
            StrokeColor = StrokeColor, StrokeWidth = StrokeWidth, TransformationMatrix = TransformationMatrix
        };
    }

    public override VectorPath ToPath()
    {
        // TODO: Apply transformation matrix
        
        VectorPath path = new VectorPath();
        path.MoveTo((VecF)Start);
        path.LineTo((VecF)End);
        return path;
    }
}
