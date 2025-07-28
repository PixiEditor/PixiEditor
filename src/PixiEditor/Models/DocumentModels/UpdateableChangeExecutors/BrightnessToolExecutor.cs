using Avalonia.Input;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class BrightnessToolExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private bool repeat;
    private float correctionFactor;
    private int toolSize;
    private bool squareBrush;

    public override ExecutionState Start()
    {
        IStructureMemberHandler? member = document!.SelectedStructureMember;
        IBrightnessToolHandler? tool = GetHandler<IBrightnessToolHandler>();
        if (tool is null || member is null || tool.Toolbar is not IToolSizeToolbar toolbar)
            return ExecutionState.Error;
        if (member is not ILayerHandler layer || layer.ShouldDrawOnMask)
            return ExecutionState.Error;

        guidValue = member.Id;
        repeat = tool.BrightnessMode == BrightnessMode.Repeat;
        toolSize = (int)toolbar.ToolSize;
        correctionFactor = tool.Darken || tool.UsedWith == MouseButton.Right ? -tool.CorrectionFactor : tool.CorrectionFactor;

        squareBrush = tool.BrushShape == PaintBrushShape.Square;

        ChangeBrightness_Action action = new(guidValue, controller!.LastPixelPosition, correctionFactor, toolSize, squareBrush, repeat, document.AnimationHandler.ActiveFrameBindable);
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        ChangeBrightness_Action action = new(guidValue, pos, correctionFactor, toolSize, squareBrush, repeat, document!.AnimationHandler.ActiveFrameBindable);
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndChangeBrightness_Action());
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndChangeBrightness_Action());
    }
}
