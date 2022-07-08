using ChunkyImageLib.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class LineBasedPenExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private SKColor color;
    private int toolSize;
    private bool drawOnMask;

    public override OneOf<Success, Error> Start()
    {
        ViewModelMain? vm = ViewModelMain.Current;
        StructureMemberViewModel? member = document!.SelectedStructureMember;
        PenToolViewModel? penTool = (PenToolViewModel?)(vm?.ToolsSubViewModel.GetTool<PenToolViewModel>());
        PenToolbar? toolbar = penTool?.Toolbar as PenToolbar;
        if (vm is null || penTool is null || member is null || toolbar is null)
            return new Error();

        guidValue = member.GuidValue;
        color = vm.ColorsSubViewModel.PrimaryColor;
        toolSize = toolbar.ToolSize;
        drawOnMask = member.ShouldDrawOnMask;

        LineBasedPen_Action? action = new(
            guidValue,
            color,
            controller!.LastPixelPosition,
            toolSize,
            false,
            drawOnMask);
        helpers!.ActionAccumulator.AddActions(action);

        return new Success();
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        LineBasedPen_Action? action = new(
            guidValue,
            color,
            pos,
            toolSize,
            false,
            drawOnMask);
        helpers!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp()
    {
        helpers!.ActionAccumulator.AddFinishedActions(new EndLineBasedPen_Action());
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        helpers!.ActionAccumulator.AddFinishedActions(new EndLineBasedPen_Action());
    }
}
