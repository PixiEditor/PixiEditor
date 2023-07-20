using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Models.Containers;
using PixiEditor.Models.Containers.Toolbars;
using PixiEditor.Models.Containers.Tools;
using PixiEditor.Models.Enums;

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
        IStructureMemberHandler? member = document!.SelectedStructureMember;
        IColorsHandler? colorsHandler = GetHandler<IColorsHandler>();

        IPenToolHandler? penTool = GetHandler<IPenToolHandler>();
        if (colorsHandler is null || penTool is null || member is null || penTool?.Toolbar is not IBasicToolbar toolbar)
            return ExecutionState.Error;
        drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        guidValue = member.GuidValue;
        color = colorsHandler.PrimaryColor;
        toolSize = toolbar.ToolSize;
        pixelPerfect = penTool.PixelPerfectEnabled;

        colorsHandler.AddSwatch(new PaletteColor(color.R, color.G, color.B));
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
