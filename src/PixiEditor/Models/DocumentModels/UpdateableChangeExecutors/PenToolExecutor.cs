using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PenToolExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private Color color;
    public int ToolSize => basicToolbar.ToolSize;
    private bool drawOnMask;
    private bool pixelPerfect;
    private bool antiAliasing;

    private IBasicToolbar basicToolbar;

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

        basicToolbar = toolbar;
        guidValue = member.Id;
        color = colorsHandler.PrimaryColor;
        pixelPerfect = penTool.PixelPerfectEnabled;
        antiAliasing = penTool.AntiAliasing;

        colorsHandler.AddSwatch(new PaletteColor(color.R, color.G, color.B));
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(guidValue, color, controller!.LastPixelPosition, ToolSize, false, antiAliasing, drawOnMask, document!.AnimationHandler.ActiveFrameBindable),
            true => new PixelPerfectPen_Action(guidValue, controller!.LastPixelPosition, color, drawOnMask, document!.AnimationHandler.ActiveFrameBindable)
        };
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(guidValue, color, pos, ToolSize, false, antiAliasing, drawOnMask, document!.AnimationHandler.ActiveFrameBindable),
            true => new PixelPerfectPen_Action(guidValue, pos, color, drawOnMask, document!.AnimationHandler.ActiveFrameBindable)
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
