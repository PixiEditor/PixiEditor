using PixiEditor.ChangeableDocument.Changes;

namespace PixiEditor.ChangeableDocument.Actions;

internal interface IStartOrUpdateChangeAction : IAction
{
    bool IsChangeTypeMatching(Change change);
    void UpdateCorrespodingChange(UpdateableChange change);
    UpdateableChange CreateCorrespondingChange();
}
