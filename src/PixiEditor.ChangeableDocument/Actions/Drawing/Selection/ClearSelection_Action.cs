using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Drawing;

namespace PixiEditor.ChangeableDocument.Actions.Drawing.Selection;

public record class ClearSelection_Action : IMakeChangeAction
{
    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new ClearSelection_Change();
    }
}
