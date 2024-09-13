using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Common;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public abstract class ShapeVectorData : ICacheable, ICloneable, IReadOnlyShapeVectorData
{
    public Matrix3X3 TransformationMatrix { get; set; } = Matrix3X3.Identity; 
    
    public Color StrokeColor { get; set; } = Colors.White;
    public Color FillColor { get; set; } = Colors.White;
    public int StrokeWidth { get; set; } = 1;
    public abstract RectD GeometryAABB { get; }
    public RectD TransformedAABB => new ShapeCorners(GeometryAABB).WithMatrix(TransformationMatrix).AABBBounds;
    public abstract ShapeCorners TransformationCorners { get; } 

    public abstract void Rasterize(DrawingSurface drawingSurface, ChunkResolution resolution, Paint? paint);
    public abstract bool IsValid();
    public abstract int GetCacheHash();
    public abstract int CalculateHash();
    public abstract object Clone();

    public override int GetHashCode()
    {
        return CalculateHash();
    }
}
