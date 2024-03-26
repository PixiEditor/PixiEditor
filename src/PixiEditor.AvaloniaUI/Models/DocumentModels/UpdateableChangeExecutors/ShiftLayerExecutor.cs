using System.Collections.Generic;
using System.Linq;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class ShiftLayerExecutor : UpdateableChangeExecutor
{
    private List<Guid> _affectedMemberGuids = new List<Guid>();
    private VecI startPos;
    private IMoveToolHandler? tool;

    public override ExecutorStartMode StartMode => ExecutorStartMode.OnMouseLeftButtonDown;

    public override ExecutionState Start()
    {
        IStructureMemberHandler? member = document!.SelectedStructureMember;
        
        tool = GetHandler<IMoveToolHandler>();
        if (tool is null)
            return ExecutionState.Error;

        if (tool.MoveAllLayers)
        {
            _affectedMemberGuids.AddRange(document.StructureHelper.GetAllLayers().Select(x => x.GuidValue));
        }
        else
        {
            if (member != null)
                _affectedMemberGuids.Add(member.GuidValue);
            _affectedMemberGuids.AddRange(document!.SoftSelectedStructureMembers.Select(x => x.GuidValue));
        }

        RemoveDrawOnMaskLayers(_affectedMemberGuids);
        
        startPos = controller!.LastPixelPosition;

        ShiftLayer_Action action = new(_affectedMemberGuids, VecI.Zero, tool.KeepOriginalImage);
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    private void RemoveDrawOnMaskLayers(List<Guid> affectedMemberGuids)
    {
        for (var i = 0; i < affectedMemberGuids.Count; i++)
        {
            var guid = affectedMemberGuids[i];
            if (document!.StructureHelper.FindOrThrow(guid) is ILayerHandler { ShouldDrawOnMask: true })
            {
                _affectedMemberGuids.Remove(guid);
                i--;
            }
        }
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        ShiftLayer_Action action = new(_affectedMemberGuids, pos - startPos, tool!.KeepOriginalImage);
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndShiftLayer_Action());
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndShiftLayer_Action());
    }
}
