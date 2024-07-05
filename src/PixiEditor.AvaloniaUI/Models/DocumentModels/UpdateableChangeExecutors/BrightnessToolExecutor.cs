using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Handlers.Toolbars;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class BrightnessToolExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private bool repeat;
    private float correctionFactor;
    private int toolSize;

    public override ExecutionState Start()
    {
        IStructureMemberHandler? member = document!.SelectedStructureMember;
        IBrightnessToolHandler? tool = GetHandler<IBrightnessToolHandler>();
        if (tool is null || member is null || tool.Toolbar is not IBasicToolbar toolbar)
            return ExecutionState.Error;
        if (member is not ILayerHandler layer || layer.ShouldDrawOnMask)
            return ExecutionState.Error;

        guidValue = member.Id;
        repeat = tool.BrightnessMode == BrightnessMode.Repeat;
        toolSize = toolbar.ToolSize;
        correctionFactor = tool.Darken || tool.UsedWith == MouseButton.Right ? -tool.CorrectionFactor : tool.CorrectionFactor;

        ChangeBrightness_Action action = new(guidValue, controller!.LastPixelPosition, correctionFactor, toolSize, repeat, document.AnimationHandler.ActiveFrameBindable);
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        ChangeBrightness_Action action = new(guidValue, pos, correctionFactor, toolSize, repeat, document!.AnimationHandler.ActiveFrameBindable);
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndChangeBrightness_Action());
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndChangeBrightness_Action());
    }
}
