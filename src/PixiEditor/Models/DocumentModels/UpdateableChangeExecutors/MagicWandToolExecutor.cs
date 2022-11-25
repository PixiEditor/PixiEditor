using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class MagicWandToolExecutor : UpdateableChangeExecutor
{
    private bool considerAllLayers;
    private bool drawOnMask;
    private Guid memberGuid;
    private SelectionMode mode;

    public override ExecutionState Start()
    {
        var magicWand = ViewModelMain.Current?.ToolsSubViewModel.GetTool<MagicWandToolViewModel>();
        var member = document!.SelectedStructureMember;

        if (magicWand is null || member is null)
            return ExecutionState.Error;
        drawOnMask = member is not LayerViewModel layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not LayerViewModel)
            return ExecutionState.Error;

        mode = magicWand.SelectMode;
        memberGuid = member.GuidValue;
        considerAllLayers = magicWand.DocumentScope == DocumentScope.AllLayers;
        var pos = controller!.LastPixelPosition;

        internals!.ActionAccumulator.AddActions(new MagicWand_Action(memberGuid, pos, mode, considerAllLayers, drawOnMask));

        return ExecutionState.Success;
    }

    public override void OnLeftMouseButtonUp()
    {
        internals!.ActionAccumulator.AddActions(new ChangeBoundary_Action());
        onEnded!(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddActions(new ChangeBoundary_Action());
    }
}
