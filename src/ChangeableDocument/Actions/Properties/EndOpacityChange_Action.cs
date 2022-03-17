using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions.Properties
{
    public record class EndOpacityChange_Action : IEndChangeAction
    {
        bool IEndChangeAction.IsChangeTypeMatching(IChange change) => change is StructureMemberOpacity_UpdateableChange;
    }
}
