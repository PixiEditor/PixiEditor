using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal interface IChange
    {
        void Initialize(IChangeable target);
        IChangeInfo? Apply(IChangeable target);
        IChangeInfo? Revert(IChangeable target);
    }
}
