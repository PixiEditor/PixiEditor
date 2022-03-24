using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal abstract class UpdateableChange : Change
    {
        public abstract IChangeInfo? ApplyTemporarily(Document target);
    }
}
