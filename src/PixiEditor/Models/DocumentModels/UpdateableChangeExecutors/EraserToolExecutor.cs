#nullable enable
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class EraserToolExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private SKColor color;
    private int toolSize;
    private bool drawOnMask;

    public override ExecutionState Start()
    {
        ViewModelMain? vm = ViewModelMain.Current;
        StructureMemberViewModel? member = document!.SelectedStructureMember;
        EraserToolViewModel? eraserTool = (EraserToolViewModel?)(vm?.ToolsSubViewModel.GetTool<EraserToolViewModel>());
        BasicToolbar? toolbar = eraserTool?.Toolbar as BasicToolbar;
        if (vm is null || eraserTool is null || member is null || toolbar is null)
            return ExecutionState.Error;
        drawOnMask = member is LayerViewModel layer ? layer.ShouldDrawOnMask : true;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not LayerViewModel)
            return ExecutionState.Error;

        guidValue = member.GuidValue;
        color = vm.ColorsSubViewModel.PrimaryColor;
        toolSize = toolbar.ToolSize;

        vm.ColorsSubViewModel.AddSwatch(color);
        IAction? action = new LineBasedPen_Action(guidValue, SKColors.Transparent, controller!.LastPixelPosition, toolSize, true,
            drawOnMask);
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        IAction? action = new LineBasedPen_Action(guidValue, SKColors.Transparent, pos, toolSize, true, drawOnMask);
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndLineBasedPen_Action());
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        IAction? action = new EndLineBasedPen_Action();
        internals!.ActionAccumulator.AddFinishedActions(action);
    }
}
