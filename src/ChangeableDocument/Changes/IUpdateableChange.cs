using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal interface IUpdateableChange : IChange
    {
        IChangeInfo? ApplyTemporarily(Document target);
    }
}
