using ChangeableDocument.Changes;
using ChangeableDocument.Changes.Drawing;

namespace ChangeableDocument.Actions.Drawing.Selection
{
    public record struct ClearSelection_Action : IMakeChangeAction
    {
        IChange IMakeChangeAction.CreateCorrespondingChange()
        {
            return new ClearSelection_Change();
        }
    }
}
