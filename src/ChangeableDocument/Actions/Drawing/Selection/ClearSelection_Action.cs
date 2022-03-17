using ChangeableDocument.Changes;
using ChangeableDocument.Changes.Drawing;

namespace ChangeableDocument.Actions.Drawing.Selection
{
    public record class ClearSelection_Action : IMakeChangeAction
    {
        IChange IMakeChangeAction.CreateCorrespondingChange()
        {
            return new ClearSelection_Change();
        }
    }
}
