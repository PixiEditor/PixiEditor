using ChangeableDocument.Changes;
using ChangeableDocument.Changes.Drawing;

namespace ChangeableDocument.Actions.Drawing.Selection
{
    public record class EndSelectRectangle_Action : IEndChangeAction
    {
        bool IEndChangeAction.IsChangeTypeMatching(Change change)
        {
            return change is SelectRectangle_UpdateableChange;
        }
    }
}
