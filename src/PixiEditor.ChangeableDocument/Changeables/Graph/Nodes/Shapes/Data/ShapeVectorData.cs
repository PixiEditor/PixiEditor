using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Common;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public abstract class ShapeVectorData : ICacheable, ICloneable, IReadOnlyShapeVectorData
{
    private float strokeWidth = 0;

    public Matrix3X3 TransformationMatrix { get; set; } = Matrix3X3.Identity;

    public Color StrokeColor { get; set; } = Colors.White;
    public Color FillColor { get; set; } = Colors.White;

    public float StrokeWidth
    {
        get => strokeWidth;
        set
        {
            strokeWidth = value;
            OnStrokeWidthChanged();
        }
    }
    
    public bool Fill { get; set; } = true;
    public abstract RectD GeometryAABB { get; }
    public abstract RectD VisualAABB { get; }
    public RectD TransformedAABB => new ShapeCorners(GeometryAABB).WithMatrix(TransformationMatrix).AABBBounds;
    public RectD TransformedVisualAABB => new ShapeCorners(VisualAABB).WithMatrix(TransformationMatrix).AABBBounds;
    public abstract ShapeCorners TransformationCorners { get; }

    protected void ApplyTransformTo(Canvas canvas)
    {
        Matrix3X3 canvasMatrix = canvas.TotalMatrix;

        Matrix3X3 final = canvasMatrix.Concat(TransformationMatrix);

        canvas.SetMatrix(final);
    }

    public abstract void RasterizeGeometry(Canvas canvas);
    public abstract void RasterizeTransformed(Canvas canvas);
    public abstract bool IsValid();
    public abstract int GetCacheHash();
    public abstract int CalculateHash();

    public object Clone()
    {
        ShapeVectorData copy = (ShapeVectorData)MemberwiseClone();
        AdjustCopy(copy);
        return copy;
    }

    protected virtual void AdjustCopy(ShapeVectorData copy) { }
    
    protected virtual void OnStrokeWidthChanged() { }

    public override int GetHashCode()
    {
        return CalculateHash();
    }

    public abstract VectorPath ToPath();
}
