using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal interface IChange : IDisposable
    {
        void Initialize(Document target);
        IChangeInfo? Apply(Document target);
        IChangeInfo? Revert(Document target);
    };
}
