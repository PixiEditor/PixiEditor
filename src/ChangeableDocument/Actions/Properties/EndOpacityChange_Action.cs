using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions.Properties
{
    public record class EndOpacityChange_Action : IEndChangeAction
    {
        bool IEndChangeAction.IsChangeTypeMatching(Change change) => change is StructureMemberOpacity_UpdateableChange;
    }
}
