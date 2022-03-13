using ChunkyImageLib;

namespace ChangeableDocument.Changeables.Interfaces
{
    public interface IReadOnlyLayer : IReadOnlyStructureMember
    {
        IReadOnlyChunkyImage ReadOnlyLayerImage { get; }
    }
}
