using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Common;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public abstract class ShapeVectorData : ICacheable, ICloneable, IReadOnlyShapeVectorData
{
    public Matrix3X3 TransformationMatrix { get; set; } = Matrix3X3.Identity; 
    
    public Color StrokeColor { get; set; } = Colors.White;
    public Color FillColor { get; set; } = Colors.White;
    public float StrokeWidth { get; set; } = 1;
    public abstract RectD GeometryAABB { get; }
    public RectD TransformedAABB => new ShapeCorners(GeometryAABB).WithMatrix(TransformationMatrix).AABBBounds;
    public abstract ShapeCorners TransformationCorners { get; } 
    
    protected void ApplyTransformTo(DrawingSurface drawingSurface)
    {
        Matrix3X3 canvasMatrix = drawingSurface.Canvas.TotalMatrix;

        Matrix3X3 final = canvasMatrix.Concat(TransformationMatrix);

        drawingSurface.Canvas.SetMatrix(final);
    }

    public abstract void RasterizeGeometry(DrawingSurface drawingSurface);
    public abstract void RasterizeTransformed(DrawingSurface drawingSurface);
    public abstract bool IsValid();
    public abstract int GetCacheHash();
    public abstract int CalculateHash();
    public abstract object Clone();

    public override int GetHashCode()
    {
        return CalculateHash();
    }
}
