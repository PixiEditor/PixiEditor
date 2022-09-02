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
    private Guid guidValue;
    private VecI startPos;
    private MoveToolToolbar? toolbar;

    public override ExecutionState Start()
    {
        ViewModelMain? vm = ViewModelMain.Current;
        StructureMemberViewModel? member = document!.SelectedStructureMember;
        toolbar = (MoveToolToolbar?)(ViewModelMain.Current?.ToolsSubViewModel.GetTool<MoveToolViewModel>()?.Toolbar);
        if (vm is null || member is not LayerViewModel layer || layer.ShouldDrawOnMask || toolbar is null)
            return ExecutionState.Error;

        guidValue = member.GuidValue;
        startPos = controller!.LastPixelPosition;

        ShiftLayer_Action action = new(guidValue, VecI.Zero, toolbar.KeepOriginalImage);
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        ShiftLayer_Action action = new(guidValue, pos - startPos, toolbar!.KeepOriginalImage);
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
