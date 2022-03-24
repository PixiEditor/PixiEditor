using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal interface IChange : IDisposable
    {
        void Initialize(Document target);
        IChangeInfo? Apply(Document target, out bool ignoreInUndo);
        IChangeInfo? Revert(Document target);
    };
}
