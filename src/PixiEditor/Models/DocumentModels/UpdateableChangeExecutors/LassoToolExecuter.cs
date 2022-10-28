using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal sealed class LassoToolExecuter : UpdateableChangeExecutor
{
    private SelectionMode? mode;
    
    public override ExecutionState Start()
    {
        mode = ((LassoToolbar)ViewModelMain.Current?.ToolsSubViewModel.GetTool<LassoToolViewModel>()?.Toolbar)?.SelectMode;

        if (mode == null)
            return ExecutionState.Error;
        
        AddStartAction(controller!.LastPixelPosition);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos) => AddStartAction(pos);

    public override void OnLeftMouseButtonUp()
    {
        internals!.ActionAccumulator.AddActions(new EndSelectLasso_Action());
        onEnded!(this);
    }

    public override void ForceStop()
    {
        OnLeftMouseButtonUp();
    }

    private void AddStartAction(VecI pos)
    {
        var action = new SelectLasso_Action(pos, mode!.Value);
        
        internals!.ActionAccumulator.AddActions(action);
    }
}
