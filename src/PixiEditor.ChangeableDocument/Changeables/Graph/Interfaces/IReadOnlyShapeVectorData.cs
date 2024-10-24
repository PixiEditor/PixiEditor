using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyShapeVectorData
{
    public Matrix3X3 TransformationMatrix { get; }
    public Color StrokeColor { get; }
    public Color FillColor { get; }
    public int StrokeWidth { get; }
    public RectD GeometryAABB { get; }
    public RectD TransformedAABB { get; }
}
