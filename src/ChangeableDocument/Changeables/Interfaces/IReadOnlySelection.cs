using ChunkyImageLib;

namespace ChangeableDocument.Changeables.Interfaces
{
    public interface IReadOnlySelection
    {
        public IReadOnlyChunkyImage ReadOnlySelectionImage { get; }
        public bool ReadOnlyIsEmptyAndInactive { get; }
    }
}
