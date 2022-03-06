using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class StructureMemberProperties_Change : IChange
    {
        private Guid memberGuid;

        private bool originalIsVisible;
        public bool? NewIsVisible { get; init; } = null;

        private string? originalName;
        public string? NewName { get; init; } = null;

        public StructureMemberProperties_Change(Guid memberGuid)
        {
            this.memberGuid = memberGuid;
        }

        public void Initialize(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            if (NewIsVisible != null) originalIsVisible = member.IsVisible;
            if (NewName != null) originalName = member.Name;
        }

        public IChangeInfo? Apply(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            if (NewIsVisible != null) member.IsVisible = NewIsVisible.Value;
            if (NewName != null) member.Name = NewName;

            return new StructureMemberProperties_ChangeInfo()
            {
                GuidValue = member.GuidValue,
                IsVisibleChanged = NewIsVisible != null,
                NameChanged = NewName != null
            };
        }

        public IChangeInfo? Revert(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            if (NewIsVisible != null) member.IsVisible = originalIsVisible;
            if (NewName != null) member.Name = originalName!;

            return new StructureMemberProperties_ChangeInfo()
            {
                GuidValue = member.GuidValue,
                IsVisibleChanged = NewIsVisible != null,
                NameChanged = NewName != null,
            };
        }
    }
}
