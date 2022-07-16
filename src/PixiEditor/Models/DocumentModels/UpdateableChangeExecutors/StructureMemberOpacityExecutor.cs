using PixiEditor.Models.Enums;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
internal class StructureMemberOpacityExecutor : UpdateableChangeExecutor
{
    private Guid memberGuid;
    public override ExecutionState Start()
    {
        if (document.SelectedStructureMember is null)
            return ExecutionState.Error;
        memberGuid = document.SelectedStructureMember.GuidValue;
        StructureMemberOpacity_Action action = new StructureMemberOpacity_Action(memberGuid, document.SelectedStructureMember.OpacityBindable);
        helpers.ActionAccumulator.AddActions(action);
        return ExecutionState.Success;
    }

    public override void OnOpacitySliderDragged(float newValue)
    {
        StructureMemberOpacity_Action action = new StructureMemberOpacity_Action(memberGuid, newValue);
        helpers.ActionAccumulator.AddActions(action);
    }

    public override void OnOpacitySliderDragEnded()
    {
        helpers.ActionAccumulator.AddFinishedActions(new EndStructureMemberOpacity_Action());
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        helpers.ActionAccumulator.AddFinishedActions(new EndStructureMemberOpacity_Action());
    }
}
