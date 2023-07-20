#nullable enable
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Models.Containers;
using PixiEditor.Models.Containers.Tools;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class EraserToolExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private Color color;
    private int toolSize;
    private bool drawOnMask;

    public override ExecutionState Start()
    {
        IStructureMemberHandler? member = document!.SelectedStructureMember;
        IEraserToolHandler? eraserTool = GetHandler<IEraserToolHandler>();
        BasicToolbar? toolbar = eraserTool?.Toolbar as BasicToolbar;
        if (vm is null || eraserTool is null || member is null || toolbar is null)
            return ExecutionState.Error;
        drawOnMask = member is ILayerHandler layer ? layer.ShouldDrawOnMask : true;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        guidValue = member.GuidValue;
        color = vm.ColorsSubViewModel.PrimaryColor;
        toolSize = toolbar.ToolSize;

        vm.ColorsSubViewModel.AddSwatch(new PaletteColor(color.R, color.G, color.B));
        IAction? action = new LineBasedPen_Action(guidValue, DrawingApi.Core.ColorsImpl.Colors.Transparent, controller!.LastPixelPosition, toolSize, true,
            drawOnMask);
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        IAction? action = new LineBasedPen_Action(guidValue, DrawingApi.Core.ColorsImpl.Colors.Transparent, pos, toolSize, true, drawOnMask);
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
