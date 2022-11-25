using PixiEditor.ChangeableDocument.Changes;

namespace PixiEditor.ChangeableDocument.Actions;

internal interface IEndChangeAction : IAction
{
    bool IsChangeTypeMatching(Change change);
}
