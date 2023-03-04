using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class ShiftLayerExecutor : UpdateableChangeExecutor
{
    private List<Guid> _affectedMemberGuids = new List<Guid>();
    private VecI startPos;
    private MoveToolViewModel? tool;

    public override ExecutorStartMode StartMode => ExecutorStartMode.OnMouseLeftButtonDown;

    public override ExecutionState Start()
    {
        ViewModelMain? vm = ViewModelMain.Current;
        StructureMemberViewModel? member = document!.SelectedStructureMember;
        
        tool = ViewModelMain.Current?.ToolsSubViewModel.GetTool<MoveToolViewModel>();
        if (vm is null || tool is null)
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
            if (document!.StructureHelper.FindOrThrow(guid) is LayerViewModel { ShouldDrawOnMask: true })
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
