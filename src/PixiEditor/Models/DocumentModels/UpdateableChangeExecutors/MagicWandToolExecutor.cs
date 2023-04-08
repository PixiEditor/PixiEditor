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
    private List<Guid> memberGuids;
    private SelectionMode mode;

    public override ExecutionState Start()
    {
        var magicWand = ViewModelMain.Current?.ToolsSubViewModel.GetTool<MagicWandToolViewModel>();
        var members = document!.ExtractSelectedLayers(true);

        if (magicWand is null || members.Count == 0)
            return ExecutionState.Error;

        mode = magicWand.SelectMode;
        memberGuids = members;
        considerAllLayers = magicWand.DocumentScope == DocumentScope.AllLayers;
        if (considerAllLayers)
            memberGuids = document!.StructureHelper.GetAllLayers().Select(x => x.GuidValue).ToList();
        var pos = controller!.LastPixelPosition;

        internals!.ActionAccumulator.AddActions(new MagicWand_Action(memberGuids, pos, mode, considerAllLayers));

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
