using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions
{
    internal interface IStartOrUpdateChangeAction : IAction
    {
        void UpdateCorrespodingChange(UpdateableChange change);
        UpdateableChange CreateCorrespondingChange();
    }
}
