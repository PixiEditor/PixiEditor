using PixiEditor.Common;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public abstract class ShapeData : ICacheable, ICloneable
{
    public Color StrokeColor { get; set; }
    public Color FillColor { get; set; }
    public int StrokeWidth { get; set; }

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
