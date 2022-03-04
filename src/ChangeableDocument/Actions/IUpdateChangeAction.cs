using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions
{
    internal interface IUpdateChangeAction
    {
        void UpdateCorrespodingChange(IUpdateableChange change);
    }
}
