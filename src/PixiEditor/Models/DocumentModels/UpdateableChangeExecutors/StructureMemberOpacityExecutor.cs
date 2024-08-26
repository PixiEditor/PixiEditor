using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
internal class StructureMemberOpacityExecutor : UpdateableChangeExecutor
{
    private Guid memberGuid;
    public override ExecutionState Start()
    {
        if (document.SelectedStructureMember is null)
            return ExecutionState.Error;
        memberGuid = document.SelectedStructureMember.Id;
        StructureMemberOpacity_Action action = new StructureMemberOpacity_Action(memberGuid, document.SelectedStructureMember.OpacityBindable);
        internals.ActionAccumulator.AddActions(action);
        return ExecutionState.Success;
    }

    public override void OnOpacitySliderDragged(float newValue)
    {
        StructureMemberOpacity_Action action = new StructureMemberOpacity_Action(memberGuid, newValue);
        internals.ActionAccumulator.AddActions(action);
    }

    public override void OnOpacitySliderDragEnded()
    {
        internals.ActionAccumulator.AddFinishedActions(new EndStructureMemberOpacity_Action());
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        internals.ActionAccumulator.AddFinishedActions(new EndStructureMemberOpacity_Action());
    }
}
