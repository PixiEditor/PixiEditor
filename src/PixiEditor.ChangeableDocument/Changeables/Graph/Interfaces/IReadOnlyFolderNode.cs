using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyFolderNode : IReadOnlyStructureNode
{
    public HashSet<Guid> GetLayerNodeGuids();
}
