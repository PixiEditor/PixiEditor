using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyShapeVectorData
{
    public Matrix3X3 TransformationMatrix { get; }
    public Color StrokeColor { get; }
    public bool Fill { get; }
    public Color FillColor { get; }
    public float StrokeWidth { get; }
    public RectD GeometryAABB { get; }
    public RectD TransformedAABB { get; }
    public ShapeCorners TransformationCorners { get; }
    public VectorPath ToPath();
}
