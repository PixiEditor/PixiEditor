#nullable enable
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

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class EraserToolExecutor : UpdateableChangeExecutor
{
    public bool SquareBrush => penToolbar.PenShape == PenBrushShape.Square;
    private Guid guidValue;
    private Color color;
    private double toolSize;
    private bool antiAliasing;
    private float hardness;
    private float spacing;
    
    private bool drawOnMask;
    private IPenToolbar penToolbar;

    public override ExecutionState Start()
    {
        IStructureMemberHandler? member = document!.SelectedStructureMember;
        IEraserToolHandler? eraserTool = GetHandler<IEraserToolHandler>();
        IPenToolbar? toolbar = eraserTool?.Toolbar as IPenToolbar;
        penToolbar = toolbar;
        IColorsHandler? colorsHandler = GetHandler<IColorsHandler>();

        if (colorsHandler is null || eraserTool is null || member is null || toolbar is null)
            return ExecutionState.Error;
        drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;


        guidValue = member.Id;
        color = GetHandler<IColorsHandler>().PrimaryColor;
        toolSize = toolbar.ToolSize;
        antiAliasing = toolbar.AntiAliasing;
        hardness = toolbar.Hardness;
        spacing = toolbar.Spacing;

        colorsHandler.AddSwatch(new PaletteColor(color.R, color.G, color.B));
        IAction? action = new LineBasedPen_Action(guidValue, Colors.White, controller!.LastPixelPosition, (float)toolSize, true,
            antiAliasing, hardness, spacing, SquareBrush, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        IAction? action = new LineBasedPen_Action(guidValue, Colors.White, pos, (float)toolSize, true, antiAliasing, hardness, spacing, SquareBrush, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
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
