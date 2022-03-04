using ChunkyImageLib;

namespace ChangeableDocument.Changeables.Interfaces
{
    public interface IReadOnlyLayer : IReadOnlyStructureMember
    {
        ChunkyImage LayerImage { get; }
    }
}
