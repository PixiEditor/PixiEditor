using ChunkyImageLib;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyLayer : IReadOnlyStructureMember
{
    IReadOnlyChunkyImage ReadOnlyLayerImage { get; }
}
