using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal sealed class LassoToolExecutor : UpdateableChangeExecutor
{
    private SelectionMode? mode;
    
    public override ExecutionState Start()
    {
        mode = ViewModelMain.Current?.ToolsSubViewModel.GetTool<LassoToolViewModel>()?.ResultingSelectionMode;

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
