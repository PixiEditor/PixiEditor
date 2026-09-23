using Drawie.Backend.Core.Text;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyTextData : IReadOnlyShapeVectorData
{
    public RichText Text { get; }
    public VecD Position { get; }
    public double MaxWidth { get; }
}
