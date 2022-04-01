using PixiEditor.ChangeableDocument.Changes;

namespace PixiEditor.ChangeableDocument.Actions
{
    internal interface IMakeChangeAction : IAction
    {
        Change CreateCorrespondingChange();
    }
}
