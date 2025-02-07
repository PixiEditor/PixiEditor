using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class PathVectorData : ShapeVectorData, IReadOnlyPathData
{
    public VectorPath Path { get; set; }
    public override RectD GeometryAABB => Path?.TightBounds ?? RectD.Empty;
    public override RectD VisualAABB => GeometryAABB.Inflate(StrokeWidth / 2);

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(GeometryAABB).WithMatrix(TransformationMatrix);

    public StrokeCap StrokeLineCap { get; set; } = StrokeCap.Round;
    
    public StrokeJoin StrokeLineJoin { get; set; } = StrokeJoin.Round;

    public PathVectorData(VectorPath path)
    {
        Path = path;
        if (path == null)
        {
            Path = new VectorPath();
        }
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
        if(Path == null)
        {
            return;
        }

        int num = 0;
        if (applyTransform)
        {
            num = canvas.Save();
            ApplyTransformTo(canvas);
        }

        using Paint paint = new Paint()
        {
            IsAntiAliased = true, StrokeJoin = StrokeLineJoin, StrokeCap = StrokeLineCap
        };

        if (Fill && FillColor.A > 0)
        {
            paint.Color = FillColor;
            paint.Style = PaintStyle.Fill;

            canvas.DrawPath(Path, paint);
        }

        if (StrokeWidth > 0)
        {
            paint.Color = StrokeColor;
            paint.Style = PaintStyle.Stroke;
            paint.StrokeWidth = StrokeWidth;
            
            canvas.DrawPath(Path, paint);
        }

        if (applyTransform)
        {
            canvas.RestoreToCount(num);
        }
    }

    public override bool IsValid()
    {
        return Path is { IsEmpty: false };
    }

    public override int GetCacheHash()
    {
        return Path.GetHashCode();
    }

    public override int CalculateHash()
    {
        return Path.GetHashCode();
    }

    protected override void AdjustCopy(ShapeVectorData copy)
    {
        if (copy is PathVectorData pathData)
        {
            pathData.Path = new VectorPath(Path);
        }
    }

    public override VectorPath ToPath()
    {
        return new VectorPath(Path);
    }
}
