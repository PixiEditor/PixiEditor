using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes
{
    internal abstract class UpdateableChange : Change
    {
        public abstract IChangeInfo? ApplyTemporarily(Document target);
    }
}
