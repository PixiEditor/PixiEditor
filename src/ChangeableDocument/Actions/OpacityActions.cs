using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions
{
    public record struct OpacityChange_Action : IStartOrUpdateChangeAction
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
            return new StructureMemberOpacity_UpdateableChange(MemberGuid, Opacity);
        }

        void IStartOrUpdateChangeAction.UpdateCorrespodingChange(IUpdateableChange change)
        {
            ((StructureMemberOpacity_UpdateableChange)change).Update(Opacity);
        }
    }

    public record struct EndOpacityChange_Action : IEndChangeAction
    {
        bool IEndChangeAction.IsChangeTypeMatching(IChange change) => change is StructureMemberOpacity_UpdateableChange;
    }
}
