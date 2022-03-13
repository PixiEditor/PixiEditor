using ChangeableDocument.Changes;
using ChangeableDocument.Changes.Drawing;

namespace ChangeableDocument.Actions.Drawing.Selection
{
    public record struct EndSelectRectangle_Action : IEndChangeAction
    {
        bool IEndChangeAction.IsChangeTypeMatching(IChange change)
        {
            return change is SelectRectangle_UpdateableChange;
        }
    }
}
