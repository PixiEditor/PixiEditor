using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PenToolExecutor : BrushBasedExecutor<IPenToolHandler>
{
    private bool pixelPerfect;

    public override ExecutionState Start()
    {
        pixelPerfect = GetHandler<IPenToolHandler>().PixelPerfectEnabled;

        if (base.Start() == ExecutionState.Error)
            return ExecutionState.Error;

        if (color.A > 0)
        {
            colorsHandler.AddSwatch(new PaletteColor(color.R, color.G, color.B));
        }

        return ExecutionState.Success;
    }

    protected override void EnqueueDrawActions(bool createLine, bool snapLineTo45Incremented = false)
    {
        var point = GetStabilizedPoint();
        point = ApplyPixelCenterSnappingAndStraightLineSnapping(point, createLine && snapLineTo45Incremented);
        
        if (createLine)
        {
            VecD prevPoint = snapToCentersOfPixels ? handler.LastAppliedPoint.Floor() + new VecD(0.5) : handler.LastAppliedPoint;
            IAction actionStart = pixelPerfect
                ? new PixelPerfectPen_Action(layerId, (VecI)prevPoint.Floor(), color, drawOnMask,
                    document!.AnimationHandler.ActiveFrameBindable)
                : new LineBasedPen_Action(layerId, prevPoint, (float)ToolSize,
                    antiAliasing, BrushData, drawOnMask,
                    document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.LastKeyboardInfo,
                    controller.EditorData);

            internals!.ActionAccumulator.AddActions(actionStart);
        }

        if (handler != null)
        {
            handler.LastAppliedPoint = point;
        }

        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(layerId, point, (float)ToolSize,
                antiAliasing, BrushData, drawOnMask,
                document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.LastKeyboardInfo,
                controller.EditorData),
            true => new PixelPerfectPen_Action(layerId, (VecI)point.Floor(), color, drawOnMask,
                document!.AnimationHandler.ActiveFrameBindable)
        };

        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnSettingsChanged(string name, object value)
    {
        base.OnSettingsChanged(name, value);
        if (name == nameof(IPenToolHandler.PixelPerfectEnabled) && value is bool bp)
        {
            EnqueueEndDraw();
            pixelPerfect = bp;
        }
    }

    protected override void EnqueueEndDraw()
    {
        firstApply = true;
        IAction? action = pixelPerfect switch
        {
            false => new EndLineBasedPen_Action(),
            true => new EndPixelPerfectPen_Action()
        };

        internals!.ActionAccumulator.AddFinishedActions(action);
    }
}
