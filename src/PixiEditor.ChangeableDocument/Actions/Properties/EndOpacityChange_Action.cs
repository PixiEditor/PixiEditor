using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Properties;

namespace PixiEditor.ChangeableDocument.Actions.Properties
{
    public record class EndOpacityChange_Action : IEndChangeAction
    {
        bool IEndChangeAction.IsChangeTypeMatching(Change change) => change is StructureMemberOpacity_UpdateableChange;
    }
}
