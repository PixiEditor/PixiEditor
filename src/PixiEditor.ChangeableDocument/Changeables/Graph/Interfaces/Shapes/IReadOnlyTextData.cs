using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyTextData
{
    public string Text { get; }
    public VecD Position { get; }
}
