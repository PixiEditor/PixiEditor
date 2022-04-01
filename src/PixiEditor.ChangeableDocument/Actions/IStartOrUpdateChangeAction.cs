using PixiEditor.ChangeableDocument.Changes;

namespace PixiEditor.ChangeableDocument.Actions
{
    internal interface IStartOrUpdateChangeAction : IAction
    {
        void UpdateCorrespodingChange(UpdateableChange change);
        UpdateableChange CreateCorrespondingChange();
    }
}
