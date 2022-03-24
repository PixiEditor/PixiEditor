using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions
{
    internal interface IEndChangeAction : IAction
    {
        bool IsChangeTypeMatching(Change change);
    }
}
