using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions
{
    internal interface IStartChangeAction : IAction
    {
        IUpdateableChange CreateCorrespondingChange();
    }
}
