using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PenToolExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private Color color;
    private int toolSize;
    private bool drawOnMask;
    private bool pixelPerfect;

    public override ExecutionState Start()
    {
        ViewModelMain? vm = ViewModelMain.Current;
        StructureMemberViewModel? member = document!.SelectedStructureMember;
        PenToolViewModel? penTool = vm?.ToolsSubViewModel.GetTool<PenToolViewModel>();
        if (vm is null || penTool is null || member is null || penTool?.Toolbar is not BasicToolbar toolbar)
            return ExecutionState.Error;
        drawOnMask = member is not LayerViewModel layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not LayerViewModel)
            return ExecutionState.Error;

        guidValue = member.GuidValue;
        color = vm.ColorsSubViewModel.PrimaryColor;
        toolSize = toolbar.ToolSize;
        pixelPerfect = penTool.PixelPerfectEnabled;

        vm.ColorsSubViewModel.AddSwatch(color);
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(guidValue, color, controller!.LastPixelPosition, toolSize, false, drawOnMask),
            true => new PixelPerfectPen_Action(guidValue, controller!.LastPixelPosition, color, drawOnMask)
        };
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(guidValue, color, pos, toolSize, false, drawOnMask),
            true => new PixelPerfectPen_Action(guidValue, pos, color, drawOnMask)
        };
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp()
    {
        IAction? action = pixelPerfect switch
        {
            false => new EndLineBasedPen_Action(),
            true => new EndPixelPerfectPen_Action()
        };

        internals!.ActionAccumulator.AddFinishedActions(action);
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        IAction? action = pixelPerfect switch
        {
            false => new EndLineBasedPen_Action(),
            true => new EndPixelPerfectPen_Action()
        };
        internals!.ActionAccumulator.AddFinishedActions(action);
    }
}
