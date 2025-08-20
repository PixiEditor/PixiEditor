using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Brushes;

public struct BrushData
{
    public IReadOnlyShapeVectorData? VectorShape { get; }

    public BrushData(IReadOnlyShapeVectorData vectorShape)
    {
        VectorShape = vectorShape;
    }
}
