using Drawie.Backend.Core;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyFolderNode : IReadOnlyStructureNode
{
    public HashSet<Guid> GetLayerNodeGuids();
    public RenderInputProperty Content { get; }
}
