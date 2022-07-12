using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PenToolExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private SKColor color;
    private int toolSize;
    private bool drawOnMask;
    private bool pixelPerfect;

    public override OneOf<Success, Error> Start()
    {
        ViewModelMain? vm = ViewModelMain.Current;
        StructureMemberViewModel? member = document!.SelectedStructureMember;
        PenToolViewModel? penTool = (PenToolViewModel?)(vm?.ToolsSubViewModel.GetTool<PenToolViewModel>());
        PenToolbar? toolbar = penTool?.Toolbar as PenToolbar;
        if (vm is null || penTool is null || member is null || toolbar is null)
            return new Error();
        drawOnMask = member.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return new Error();
        if (!drawOnMask && member is not LayerViewModel)
            return new Error();

        guidValue = member.GuidValue;
        color = vm.ColorsSubViewModel.PrimaryColor;
        toolSize = toolbar.ToolSize;
        pixelPerfect = toolbar.PixelPerfectEnabled;

        vm.ColorsSubViewModel.AddSwatch(color);
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(guidValue, color, controller!.LastPixelPosition, toolSize, false, drawOnMask),
            true => new PixelPerfectPen_Action(guidValue, controller!.LastPixelPosition, color, drawOnMask)
        };
        helpers!.ActionAccumulator.AddActions(action);

        return new Success();
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(guidValue, color, pos, toolSize, false, drawOnMask),
            true => new PixelPerfectPen_Action(guidValue, pos, color, drawOnMask)
        };
        helpers!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp()
    {
        IAction? action = pixelPerfect switch
        {
            false => new EndLineBasedPen_Action(),
            true => new EndPixelPerfectPen_Action()
        };

        helpers!.ActionAccumulator.AddFinishedActions(action);
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        IAction? action = pixelPerfect switch
        {
            false => new EndLineBasedPen_Action(),
            true => new EndPixelPerfectPen_Action()
        };
        helpers!.ActionAccumulator.AddFinishedActions(action);
    }
}
