using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions
{
    internal interface IStartOrUpdateChangeAction : IAction
    {
        void UpdateCorrespodingChange(IUpdateableChange change);
        IUpdateableChange CreateCorrespondingChange();
    }
}
