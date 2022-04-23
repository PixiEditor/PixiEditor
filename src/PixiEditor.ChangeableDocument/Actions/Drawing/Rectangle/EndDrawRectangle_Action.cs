using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Drawing;

namespace PixiEditor.ChangeableDocument.Actions.Drawing.Rectangle;

public record class EndDrawRectangle_Action : IEndChangeAction
{
    bool IEndChangeAction.IsChangeTypeMatching(Change change)
    {
        return change is DrawRectangle_UpdateableChange;
    }
}
