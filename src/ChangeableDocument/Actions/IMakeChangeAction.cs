using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions
{
    internal interface IMakeChangeAction : IAction
    {
        Change CreateCorrespondingChange();
    }
}
