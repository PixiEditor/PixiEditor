using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Properties;

namespace PixiEditor.ChangeableDocument.Actions.Properties
{
    public record class DeleteStructureMemberMask_Action : IMakeChangeAction
    {
        public Guid GuidValue { get; }

        public DeleteStructureMemberMask_Action(Guid guidValue)
        {
            GuidValue = guidValue;
        }

        Change IMakeChangeAction.CreateCorrespondingChange()
        {
            return new DeleteStructureMemberMask_Change(GuidValue);
        }
    }
}
