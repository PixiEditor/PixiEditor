using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Drawing;

namespace PixiEditor.ChangeableDocument.Actions.Drawing.PasteImage;
public record class EndPasteImage_Action : IEndChangeAction
{
    bool IEndChangeAction.IsChangeTypeMatching(Change change)
    {
        return change is PasteImage_UpdateableChange;
    }
}
