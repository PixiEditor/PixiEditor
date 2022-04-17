using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Properties;

namespace PixiEditor.ChangeableDocument.Actions.Properties
{
    public record class CreateStructureMemberMask_Action : IMakeChangeAction
    {
        public Guid GuidValue { get; }

        public CreateStructureMemberMask_Action(Guid guidValue)
        {
            GuidValue = guidValue;
        }

        Change IMakeChangeAction.CreateCorrespondingChange()
        {
            return new CreateStructureMemberMask_Change(GuidValue);
        }
    }
}
