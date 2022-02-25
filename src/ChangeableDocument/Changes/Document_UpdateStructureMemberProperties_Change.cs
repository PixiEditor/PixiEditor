using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class Document_UpdateStructureMemberProperties_Change : Change<Document>
    {
        private Guid memberGuid;

        private bool originalIsVisible;
        public bool? NewIsVisible { get; init; } = null;

        private string? originalName;
        public string? NewName { get; init; } = null;

        public Document_UpdateStructureMemberProperties_Change(Guid memberGuid)
        {
            this.memberGuid = memberGuid;
        }

        protected override void DoInitialize(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            if (NewIsVisible != null) originalIsVisible = member.IsVisible;
            if (NewName != null) originalName = member.Name;
        }

        protected override IChangeInfo? DoApply(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            if (NewIsVisible != null) member.IsVisible = NewIsVisible.Value;
            if (NewName != null) member.Name = NewName;

            return new Document_UpdateStructureMemberProperties_ChangeInfo()
            {
                GuidValue = member.GuidValue,
                IsVisibleChanged = NewIsVisible != null,
                NameChanged = NewName != null
            };
        }

        protected override IChangeInfo? DoRevert(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            if (NewIsVisible != null) member.IsVisible = originalIsVisible;
            if (NewName != null) member.Name = originalName!;

            return new Document_UpdateStructureMemberProperties_ChangeInfo()
            {
                GuidValue = member.GuidValue,
                IsVisibleChanged = NewIsVisible != null,
                NameChanged = NewName != null,
            };
        }
    }
}
