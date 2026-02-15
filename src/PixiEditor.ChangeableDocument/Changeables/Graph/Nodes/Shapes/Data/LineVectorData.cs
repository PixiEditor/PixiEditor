using System.Diagnostics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class LineVectorData : ShapeVectorData, IReadOnlyLineData
{
    public VecD Start { get; set; }
    public VecD End { get; set; }

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


    public LineVectorData(VecD startPos, VecD endPos)
    {
        Start = startPos;
        End = endPos;

        Fill = false;
    }

    public override void RasterizeGeometry(Canvas canvas)
    {
        Rasterize(canvas, false);
    }

    public override void RasterizeTransformed(Canvas canvas)
    {
        Rasterize(canvas, true);
    }

    private void Rasterize(Canvas canvas, bool applyTransform)
    {
        int num = 0;
        if (applyTransform)
        {
            num = canvas.Save();
            ApplyTransformTo(canvas);
        }

        using Paint paint = new Paint() { IsAntiAliased = true };

        paint.SetPaintable(Stroke);
        paint.Style = PaintStyle.Stroke;
        paint.StrokeWidth = StrokeWidth;

        canvas.DrawLine(Start, End, paint);

        if (applyTransform)
        {
            canvas.RestoreToCount(num);
        }
    }

    public override bool IsValid()
    {
        return Start != End;
    }

    protected override int GetSpecificHash()
    {
        HashCode hash = new();
        hash.Add(Start);
        hash.Add(End);
        return hash.ToHashCode();
    }

    public override VectorPath ToPath(bool transformed = false)
    {
        VectorPath path = new VectorPath();
        path.MoveTo((VecF)Start);
        path.LineTo((VecF)End);
        if (transformed)
        {
            path.Transform(TransformationMatrix);
        }

        return path;
    }

    public override PathVectorData? ExpandStroke()
    {
        if (StrokeWidth <= 0)
        {
            return new PathVectorData(ToPath())
            {
                Fill = this.Fill,
                Stroke = this.Stroke,
                StrokeWidth = this.StrokeWidth,
                TransformationMatrix = this.TransformationMatrix,
                FillType = PathFillType.Winding
            };
        }

        VectorPath path = new VectorPath();

        ShapeCorners corners = new ShapeCorners(Start, End, StrokeWidth);
        path.MoveTo((VecF)corners.TopLeft);
        path.LineTo((VecF)corners.TopRight);
        path.LineTo((VecF)corners.BottomRight);
        path.LineTo((VecF)corners.BottomLeft);
        path.Close();

        return new PathVectorData(path)
        {
            Fill = true,
            Stroke = this.Stroke,
            StrokeWidth = 0,
            TransformationMatrix = this.TransformationMatrix,
            FillType = PathFillType.Winding
        };
    }

    protected bool Equals(LineVectorData other)
    {
        return base.Equals(other) && Start.Equals(other.Start) && End.Equals(other.End);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((LineVectorData)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Start, End);
    }
}
