using PixiEditor.Common;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public abstract class ShapeVectorData : ICacheable, ICloneable
{
    public VecD Position { get; set; }
    public VecD Size { get; set; } 
    
    /// <summary>
    ///     Rotation in radians.
    /// </summary>
    public double RotationRadians { get; set; }
    
    public Color StrokeColor { get; set; } = Colors.White;
    public Color FillColor { get; set; } = Colors.White;
    public int StrokeWidth { get; set; } = 1;
    public abstract RectD AABB { get; }

    public abstract void Rasterize(DrawingSurface drawingSurface);
    public abstract bool IsValid();
    public abstract int GetCacheHash();
    public abstract int CalculateHash();
    public abstract object Clone();

    public override int GetHashCode()
    {
        return CalculateHash();
    }
}
