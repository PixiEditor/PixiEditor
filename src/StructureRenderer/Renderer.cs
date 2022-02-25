using ChangeableDocument;
using ChangeableDocument.ChangeInfos;

namespace StructureRenderer
{
    public class Renderer
    {
        private DocumentChangeTracker tracker;
        public Renderer(DocumentChangeTracker tracker)
        {
            this.tracker = tracker;
        }

        public async Task<List<IChangeInfo>> ProcessChanges(IReadOnlyList<IChangeInfo> changes)
        {
            throw new NotImplementedException();
        }
    }
}
