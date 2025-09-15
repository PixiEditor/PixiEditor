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
    private Dictionary<ChunkResolution, VectorPath> simplifiedPaths = new();
    private VectorPath path;

    public VectorPath Path
    {
        get => path;
        set
        {
            path = value;
            RecalculateSimplifiedPaths();
        }
    }
    public override RectD GeometryAABB => Path?.TightBounds ?? RectD.Empty;
    public override RectD VisualAABB => GeometryAABB.Inflate(StrokeWidth / 2);

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(Path.TightBounds).WithMatrix(TransformationMatrix);

    public StrokeCap StrokeLineCap { get; set; } = StrokeCap.Round;

    public StrokeJoin StrokeLineJoin { get; set; } = StrokeJoin.Round;

    public PathFillType FillType
    {
        get => Path?.FillType ?? PathFillType.Winding;
        set
        {
            if (Path == null)
            {
                return;
            }

            Path.FillType = value;
        }
    }

    public PathVectorData(VectorPath path)
    {
        Path = path;
        if (path == null)
        {
            Path = new VectorPath();
        }
    }

    public override void RasterizeGeometry(Canvas canvas, ChunkResolution resolution)
    {
        Rasterize(canvas, false, resolution);
    }

    public override void RasterizeTransformed(Canvas canvas, ChunkResolution resolution)
    {
        Rasterize(canvas, true, resolution);
    }

    private void Rasterize(Canvas canvas, bool applyTransform, ChunkResolution resolution)
    {
        if (Path == null)
        {
            return;
        }

        int num = 0;
        if (applyTransform)
        {
            num = canvas.Save();
            ApplyTransformTo(canvas);
        }

        var finalPath = simplifiedPaths[resolution];

        using Paint paint = new Paint()
        {
            IsAntiAliased = true, StrokeJoin = StrokeLineJoin, StrokeCap = StrokeLineCap
        };

        if (Fill && FillPaintable.AnythingVisible)
        {
            paint.SetPaintable(FillPaintable);
            paint.Style = PaintStyle.Fill;

            canvas.DrawPath(finalPath, paint);
        }

        if (StrokeWidth > 0 && Stroke.AnythingVisible)
        {
            paint.SetPaintable(Stroke);
            paint.Style = PaintStyle.Stroke;
            paint.StrokeWidth = StrokeWidth;

            canvas.DrawPath(finalPath, paint);
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

    protected override int GetSpecificHash()
    {
        HashCode hash = new();
        hash.Add(Path);
        hash.Add(StrokeLineCap);
        hash.Add(StrokeLineJoin);
        hash.Add(FillType);

        return hash.ToHashCode();
    }

    protected override void AdjustCopy(ShapeVectorData copy)
    {
        if (copy is PathVectorData pathData)
        {
            pathData.Path = new VectorPath(Path);
        }
    }

    public override VectorPath ToPath(bool transformed = false)
    {
        VectorPath newPath = new VectorPath(Path);
        if (transformed)
        {
            newPath.Transform(TransformationMatrix);
        }

        return newPath;
    }

    private void RecalculateSimplifiedPaths()
    {
        foreach (var sPath in simplifiedPaths.Values)
        {
            if (!Equals(sPath, Path))
            {
                sPath.Dispose();
            }
        }

        simplifiedPaths.Clear();

        simplifiedPaths[ChunkResolution.Full] = Path;
        var half = Path.Simplify();
        simplifiedPaths[ChunkResolution.Half] = half;
        var quarter = half.Simplify();
        simplifiedPaths[ChunkResolution.Quarter] = quarter;
        var eighth = quarter.Simplify();
        simplifiedPaths[ChunkResolution.Eighth] = eighth;
    }

    protected bool Equals(PathVectorData other)
    {
        return base.Equals(other) && Path.Equals(other.Path) && StrokeLineCap == other.StrokeLineCap &&
               StrokeLineJoin == other.StrokeLineJoin
               && FillType == other.FillType;
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

        return Equals((PathVectorData)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Path, (int)StrokeLineCap, (int)StrokeLineJoin, FillType);
    }
}
