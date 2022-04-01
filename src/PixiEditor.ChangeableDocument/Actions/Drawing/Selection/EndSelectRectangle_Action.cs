using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Drawing;

namespace PixiEditor.ChangeableDocument.Actions.Drawing.Selection
{
    public record class EndSelectRectangle_Action : IEndChangeAction
    {
        bool IEndChangeAction.IsChangeTypeMatching(Change change)
        {
            return change is SelectRectangle_UpdateableChange;
        }
    }
}
