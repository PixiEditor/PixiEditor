using Drawie.Backend.Core.Text;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyTextData : IReadOnlyShapeVectorData
{
    public string Text { get; }
    public VecD Position { get; }
    public Font ConstructFont();
    public double Spacing { get; }
    public double MaxWidth { get; }
}
