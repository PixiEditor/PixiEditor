using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Root;

namespace PixiEditor.ChangeableDocument.Actions.Root.SymmetryPosition;
public class EndSetSymmetryAxisPosition_Action : IEndChangeAction
{
    bool IEndChangeAction.IsChangeTypeMatching(Change change)
    {
        return change is SymmetryAxisPosition_UpdateableChange;
    }
}
