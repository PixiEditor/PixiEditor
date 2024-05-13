using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.UpdateableChangeExecutors;

internal sealed class LassoToolExecutor : UpdateableChangeExecutor
{
    private SelectionMode? mode;
    
    public override ExecutionState Start()
    {
        mode = GetHandler<ILassoToolHandler>()?.ResultingSelectionMode;

        if (mode is null)
            return ExecutionState.Error;
        
        AddStartAction(controller!.LastPixelPosition);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos) => AddStartAction(pos);

    public override void OnLeftMouseButtonUp()
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
        var action = new SelectLasso_Action(pos, mode!.Value);
        
        internals!.ActionAccumulator.AddActions(action);
    }
}
