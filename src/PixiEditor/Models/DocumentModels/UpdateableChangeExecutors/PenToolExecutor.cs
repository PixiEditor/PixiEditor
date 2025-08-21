using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.Models.Controllers.InputDevice;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PenToolExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private Color color;
    public double ToolSize => penToolbar.ToolSize;
    public bool SquareBrush => penToolbar.PaintShape == PaintBrushShape.Square;

    private bool drawOnMask;
    private bool pixelPerfect;
    private bool antiAliasing;
    private float hardness;
    private float spacing = 1;
    private bool transparentErase;

    private Guid brushOutputGuid = Guid.Empty;

    private IPenToolbar penToolbar;

    public override ExecutionState Start()
    {
        IStructureMemberHandler? member = document!.SelectedStructureMember;
        IColorsHandler? colorsHandler = GetHandler<IColorsHandler>();

        IPenToolHandler? penTool = GetHandler<IPenToolHandler>();
        if (colorsHandler is null || penTool is null || member is null || penTool?.Toolbar is not IPenToolbar toolbar)
            return ExecutionState.Error;
        drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        penToolbar = toolbar;
        guidValue = member.Id;
        color = colorsHandler.PrimaryColor;
        pixelPerfect = penTool.PixelPerfectEnabled;
        antiAliasing = toolbar.AntiAliasing;
        hardness = toolbar.Hardness;
        spacing = toolbar.Spacing;

        if (color.A > 0)
        {
            colorsHandler.AddSwatch(new PaletteColor(color.R, color.G, color.B));
        }

        brushOutputGuid = document.NodeGraphHandler.AllNodes.FirstOrDefault(x => x.InternalName == "PixiEditor.BrushOutput")?.Id ?? Guid.Empty;

        transparentErase = color.A == 0;
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(guidValue, color, controller!.LastPixelPosition, (float)ToolSize, transparentErase, antiAliasing, hardness, spacing, brushOutputGuid, drawOnMask, document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo),
            true => new PixelPerfectPen_Action(guidValue, controller!.LastPixelPosition, color, drawOnMask, document!.AnimationHandler.ActiveFrameBindable)
        };
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos, MouseOnCanvasEventArgs args)
    {
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(guidValue, color, pos, (float)ToolSize, transparentErase, antiAliasing, hardness, spacing, brushOutputGuid, drawOnMask, document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo),
            true => new PixelPerfectPen_Action(guidValue, pos, color, drawOnMask, document!.AnimationHandler.ActiveFrameBindable)
        };
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
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
