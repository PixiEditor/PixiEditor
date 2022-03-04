using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions
{
    internal interface IMakeChangeAction : IAction
    {
        IChange CreateCorrespondingChange();
    }
}
