using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal sealed class LassoToolExecutor : UpdateableChangeExecutor
{
    private SelectionMode? mode;
    private string renderOutput;

    private VecI lastPosition;
    
    public override ExecutionState Start()
    {
        mode = GetHandler<ILassoToolHandler>()?.ResultingSelectionMode;
        renderOutput = GetHandler<WindowViewModel>().LastActiveViewport?.RenderOutputName ?? string.Empty;

        if (mode is null)
            return ExecutionState.Error;

        lastPosition = (VecI)controller!.LastPrecisePosition.Round();
        AddStartAction(lastPosition);

        return ExecutionState.Success;
    }

    public override void OnPrecisePositionChange(MouseOnCanvasEventArgs args)
    {
        VecI newPosition = (VecI)args.Point.PositionOnCanvas.Round();
        if (lastPosition != newPosition)
        {
            lastPosition = newPosition;
            AddStartAction(newPosition);
        }
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndSelectLasso_Action());
        onEnded!(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndSelectLasso_Action());
    }

    private void AddStartAction(VecI pos)
    {
        var action = new SelectLasso_Action(pos, mode!.Value, renderOutput);
        
        internals!.ActionAccumulator.AddActions(action);
    }
}
