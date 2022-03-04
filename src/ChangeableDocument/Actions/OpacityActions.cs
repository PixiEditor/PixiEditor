using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions
{
    public record OpacityChange_Action : IStartOrUpdateChangeAction
    {
        public OpacityChange_Action(Guid memberGuid, float opacity)
        {
            Opacity = opacity;
            MemberGuid = memberGuid;
        }

        public Guid MemberGuid { get; }
        public float Opacity { get; }

        IUpdateableChange IStartOrUpdateChangeAction.CreateCorrespondingChange()
        {
            return new UpdateStructureMemberOpacity_Change(MemberGuid, Opacity);
        }

        void IStartOrUpdateChangeAction.UpdateCorrespodingChange(IUpdateableChange change)
        {
            ((UpdateStructureMemberOpacity_Change)change).Update(Opacity);
        }
    }

    public record EndOpacityChange_Action : IEndChangeAction
    {
        bool IEndChangeAction.IsChangeTypeMatching(IChange change) => change is UpdateStructureMemberOpacity_Change;
    }
}
